using System.Collections.Concurrent;
using WarehousePacking.Contracts.DTOs;

namespace WarehousePacking.Server.Services
{
    public enum PackingSessionEventType
    {
        StateChanged,
        OperatorJoined,
        OperatorLeft,
        MainOperatorChanged,
        PackageSwitched,
        SessionClosed
    }

    public sealed class PackingSessionEvent
    {
        public int PackageId { get; init; }
        public int? PreviousPackageId { get; init; }
        public PackingSessionEventType EventType { get; init; }
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// Operator whose action raised this. The service is a singleton, so
        /// every circuit in the session is notified — including the one that
        /// just acted, which must not be told about its own doing.
        /// </summary>
        public string TriggeredBy { get; init; } = string.Empty;
    }

    public sealed class PackingSessionSnapshot
    {
        public int PackageId { get; init; }
        public string MainOperator { get; init; } = string.Empty;
        public List<string> ActiveOperators { get; init; } = new();
        public List<JlItemDto> JlItems { get; init; } = new();
        public List<JlItemDto> PackedItems { get; init; } = new();
    }

    public sealed class PackingOperationResult
    {
        public bool Success { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public PackingSessionSnapshot? Snapshot { get; init; }
    }

    public sealed class PackingJoinResult
    {
        public PackingSessionSnapshot Snapshot { get; init; } = new();
        public bool IsMainOperator { get; init; }
        public string JoinMessage { get; init; } = string.Empty;
    }

    public class PackingCollaborationService
    {
        private sealed class SessionState
        {
            public int PackageId { get; set; }
            public string MainOperator { get; set; } = string.Empty;
            public HashSet<string> Operators { get; } = new(StringComparer.OrdinalIgnoreCase);
            public List<JlItemDto> JlItems { get; set; } = new();
            public List<JlItemDto> PackedItems { get; set; } = new();
            public SemaphoreSlim Gate { get; } = new(1, 1);
        }

        private readonly ConcurrentDictionary<int, SessionState> _sessions = new();

        public event Func<PackingSessionEvent, Task>? SessionUpdated;

        public async Task<PackingJoinResult> JoinSessionAsync(int packageId, string username, IEnumerable<JlItemDto> jlItems, IEnumerable<JlItemDto> packedItems)
        {
            var session = _sessions.GetOrAdd(packageId, _ => new SessionState
            {
                PackageId = packageId,
                MainOperator = username,
                JlItems = CloneItems(jlItems),
                PackedItems = CloneItems(packedItems)
            });

            bool joinedNow;
            await session.Gate.WaitAsync();
            try
            {
                joinedNow = session.Operators.Add(username);
            }
            finally
            {
                session.Gate.Release();
            }

            if (joinedNow)
            {
                await RaiseEvent(new PackingSessionEvent
                {
                    PackageId = packageId,
                    EventType = PackingSessionEventType.OperatorJoined,
                    TriggeredBy = username,
                    Message = $"Operator {username} dołączył do pakowania."
                });
            }

            return new PackingJoinResult
            {
                Snapshot = CreateSnapshot(session),
                IsMainOperator = string.Equals(session.MainOperator, username, StringComparison.OrdinalIgnoreCase),
                JoinMessage = joinedNow ? string.Empty : $"Operator {username} już uczestniczy w tej paczce."
            };
        }

        public async Task<PackingOperationResult> PackAsync(int packageId, string itemCode, int documentId, int erpPositionNumber, string jlCode, decimal qty, string packingUser, Func<Task<bool>> persistCallback)
        {
            if (!_sessions.TryGetValue(packageId, out var session))
                return new PackingOperationResult { Success = false, ErrorMessage = "Sesja pakowania nie istnieje." };

            await session.Gate.WaitAsync();
            try
            {
                var source = session.JlItems.FirstOrDefault(i => Match(i, itemCode, documentId, erpPositionNumber, jlCode));
                if (source == null)
                    return new PackingOperationResult { Success = false, ErrorMessage = "Towar został już spakowany przez innego operatora." };

                if (qty <= 0 || qty > source.JlQuantity)
                    return new PackingOperationResult { Success = false, ErrorMessage = "Nieprawidłowa ilość do spakowania." };

                var persisted = await persistCallback();
                if (!persisted)
                    return new PackingOperationResult { Success = false, ErrorMessage = "Nie udało się zapisać pozycji pakowania." };

                MoveToPacked(session, source, qty, packingUser);
                var snapshot = CreateSnapshot(session);

                await RaiseEvent(new PackingSessionEvent
                {
                    PackageId = packageId,
                    EventType = PackingSessionEventType.StateChanged,
                    Message = "Stan pakowania został zaktualizowany."
                });

                return new PackingOperationResult { Success = true, Snapshot = snapshot };
            }
            finally
            {
                session.Gate.Release();
            }
        }

        public async Task<PackingOperationResult> UnpackAsync(int packageId, string itemCode, int documentId, int erpPositionNumber, string jlCode, decimal qty, string packingUser, Func<Task<bool>> persistCallback)
        {
            if (!_sessions.TryGetValue(packageId, out var session))
                return new PackingOperationResult { Success = false, ErrorMessage = "Sesja pakowania nie istnieje." };

            await session.Gate.WaitAsync();
            try
            {
                var packed = session.PackedItems.FirstOrDefault(i =>
                    Match(i, itemCode, documentId, erpPositionNumber, jlCode)
                    && string.Equals(i.PackingUser, packingUser, StringComparison.OrdinalIgnoreCase));
                if (packed == null)
                    return new PackingOperationResult { Success = false, ErrorMessage = "Możesz usunąć tylko pozycje spakowane przez siebie." };

                if (qty <= 0 || qty > packed.JlQuantity)
                    return new PackingOperationResult { Success = false, ErrorMessage = "Nieprawidłowa ilość do rozpakowania." };

                var persisted = await persistCallback();
                if (!persisted)
                    return new PackingOperationResult { Success = false, ErrorMessage = "Nie udało się usunąć pozycji pakowania." };

                MoveToLeft(session, packed, qty);
                var snapshot = CreateSnapshot(session);

                await RaiseEvent(new PackingSessionEvent
                {
                    PackageId = packageId,
                    EventType = PackingSessionEventType.StateChanged,
                    Message = "Stan pakowania został zaktualizowany."
                });

                return new PackingOperationResult { Success = true, Snapshot = snapshot };
            }
            finally
            {
                session.Gate.Release();
            }
        }

        public async Task<(bool Success, string ErrorMessage)> SwitchPackageAsync(int oldPackageId, int newPackageId, string username)
        {
            if (!_sessions.TryGetValue(oldPackageId, out var session))
                return (false, "Sesja pakowania nie istnieje.");

            await session.Gate.WaitAsync();
            try
            {
                if (!string.Equals(session.MainOperator, username, StringComparison.OrdinalIgnoreCase))
                    return (false, "Tylko główny operator może przejść do następnej paczki.");

                session.PackageId = newPackageId;
                session.PackedItems.Clear();

                _sessions.TryRemove(oldPackageId, out _);
                _sessions[newPackageId] = session;
            }
            finally
            {
                session.Gate.Release();
            }

            await RaiseEvent(new PackingSessionEvent
            {
                PackageId = newPackageId,
                PreviousPackageId = oldPackageId,
                EventType = PackingSessionEventType.PackageSwitched,
                TriggeredBy = username,
                Message = "Rozpoczęto pakowanie kolejnej paczki."
            });

            return (true, string.Empty);
        }

        public async Task LeaveSessionAsync(int packageId, string username, bool closeSession)
        {
            if (!_sessions.TryGetValue(packageId, out var session))
                return;

            bool removed;
            string previousMain;
            string newMain = string.Empty;

            await session.Gate.WaitAsync();
            try
            {
                previousMain = session.MainOperator;
                removed = session.Operators.Remove(username);

                if (closeSession)
                {
                    session.Operators.Clear();
                }
                else if (string.Equals(previousMain, username, StringComparison.OrdinalIgnoreCase) && session.Operators.Any())
                {
                    newMain = session.Operators.OrderBy(x => x).First();
                    session.MainOperator = newMain;
                }
            }
            finally
            {
                session.Gate.Release();
            }

            if (!removed && !closeSession)
                return;

            if (closeSession || !session.Operators.Any())
            {
                _sessions.TryRemove(packageId, out _);
                await RaiseEvent(new PackingSessionEvent
                {
                    PackageId = packageId,
                    EventType = PackingSessionEventType.SessionClosed,
                    TriggeredBy = username,
                    Message = "Pakowanie tej paczki zostało zakończone."
                });
                return;
            }

            await RaiseEvent(new PackingSessionEvent
            {
                PackageId = packageId,
                EventType = PackingSessionEventType.OperatorLeft,
                TriggeredBy = username,
                Message = $"Operator {username} opuścił pakowanie."
            });

            if (!string.IsNullOrWhiteSpace(newMain))
            {
                await RaiseEvent(new PackingSessionEvent
                {
                    PackageId = packageId,
                    EventType = PackingSessionEventType.MainOperatorChanged,
                    TriggeredBy = username,
                    Message = $"Nowy główny operator: {newMain}."
                });
            }
        }

        public PackingSessionSnapshot? GetSnapshot(int packageId)
        {
            if (!_sessions.TryGetValue(packageId, out var session))
                return null;

            return CreateSnapshot(session);
        }

        private static bool Match(JlItemDto item, string itemCode, int documentId, int erpPositionNumber, string jlCode)
        {
            return string.Equals(item.ItemCode, itemCode, StringComparison.OrdinalIgnoreCase)
                   && item.DocumentId == documentId
                   && item.ErpPositionNumber == erpPositionNumber
                   && string.Equals(item.JlCode, jlCode, StringComparison.OrdinalIgnoreCase);
        }

        private static void MoveToPacked(SessionState session, JlItemDto item, decimal qty, string packingUser)
        {
            var existingPacked = session.PackedItems.FirstOrDefault(p =>
                Match(p, item.ItemCode, item.DocumentId, item.ErpPositionNumber, item.JlCode)
                && string.Equals(p.PackingUser, packingUser, StringComparison.OrdinalIgnoreCase));

            if (existingPacked == null)
            {
                existingPacked = CloneItem(item);
                existingPacked.JlQuantity = 0;
                existingPacked.PackingUser = packingUser;
                session.PackedItems.Add(existingPacked);
            }

            existingPacked.JlQuantity += qty;
            item.JlQuantity -= qty;

            if (item.JlQuantity <= 0)
                session.JlItems.Remove(item);
        }

        private static void MoveToLeft(SessionState session, JlItemDto packed, decimal qty)
        {
            var existingLeft = session.JlItems.FirstOrDefault(j => Match(j, packed.ItemCode, packed.DocumentId, packed.ErpPositionNumber, packed.JlCode));
            if (existingLeft == null)
            {
                existingLeft = CloneItem(packed);
                existingLeft.JlQuantity = 0;
                session.JlItems.Add(existingLeft);
            }

            existingLeft.JlQuantity += qty;
            packed.JlQuantity -= qty;

            if (packed.JlQuantity <= 0)
                session.PackedItems.Remove(packed);
        }

        private static PackingSessionSnapshot CreateSnapshot(SessionState session)
        {
            return new PackingSessionSnapshot
            {
                PackageId = session.PackageId,
                MainOperator = session.MainOperator,
                ActiveOperators = session.Operators.OrderBy(x => x).ToList(),
                JlItems = CloneItems(session.JlItems),
                PackedItems = CloneItems(session.PackedItems)
            };
        }

        private static List<JlItemDto> CloneItems(IEnumerable<JlItemDto> items) => items.Select(CloneItem).ToList();

        private static JlItemDto CloneItem(JlItemDto source)
        {
            return new JlItemDto
            {
                JlCode = source.JlCode,
                JlEanCode = source.JlEanCode,
                ItemErpId = source.ItemErpId,
                ItemWmsId = source.ItemWmsId,
                ErpPositionNumber = source.ErpPositionNumber,
                ItemCode = source.ItemCode,
                ItemName = source.ItemName,
                ItemEan = source.ItemEan?.ToList() ?? new List<string>(),
                ItemUnit = source.ItemUnit,
                ItemType = source.ItemType,
                ItemImage = source.ItemImage,
                ItemWeight = source.ItemWeight,
                ItemVolume = source.ItemVolume,
                PackedWMS = source.PackedWMS,
                SupplierCode = source.SupplierCode?.ToList() ?? new List<string>(),
                ClientErpId = source.ClientErpId,
                AddressName = source.AddressName,
                AddressCity = source.AddressCity,
                AddressStreet = source.AddressStreet,
                AddressPostalCode = source.AddressPostalCode,
                AddressCountry = source.AddressCountry,
                ClientName = source.ClientName,
                ClientSymbol = source.ClientSymbol,
                DestinationCountry = source.DestinationCountry,
                PackingRequirements = source.PackingRequirements,
                ScanDate = source.ScanDate,
                CourierName = source.CourierName,
                Courier = source.Courier,
                LogoCourier = source.LogoCourier,
                ShipmentServices = new ShipmentServices
                {
                    D10 = source.ShipmentServices?.D10 ?? false,
                    D12 = source.ShipmentServices?.D12 ?? false,
                    Saturday = source.ShipmentServices?.Saturday ?? false,
                    PZ = source.ShipmentServices?.PZ ?? false,
                    Dropshipping = source.ShipmentServices?.Dropshipping ?? false
                },
                BatchId = source.BatchId,
                BatchNumber = source.BatchNumber,
                TermValidity = source.TermValidity,
                ErpDocumentId = source.ErpDocumentId,
                DocumentId = source.DocumentId,
                DocumentType = source.DocumentType,
                DocumentQuantity = source.DocumentQuantity,
                JlQuantity = source.JlQuantity,
                PackingUser = source.PackingUser
            };
        }

        private async Task RaiseEvent(PackingSessionEvent data)
        {
            var handler = SessionUpdated;
            if (handler == null)
                return;

            var delegates = handler.GetInvocationList().Cast<Func<PackingSessionEvent, Task>>();
            foreach (var callback in delegates)
            {
                await callback(data);
            }
        }
    }
}