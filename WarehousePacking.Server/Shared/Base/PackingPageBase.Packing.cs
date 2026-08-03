using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Server.Helpers;
using WarehousePacking.Server.Services;
using WarehousePacking.Server.Settings;
using WarehousePacking.Server.Shared.Components;
using WarehousePacking.Server.Shared.Components.Modals;
using WarehousePacking.Server.Shared.Components.Packing;

namespace WarehousePacking.Server.Shared.Base
{
    /// <summary>
    /// Scanning and moving items between the "to pack" and "packed" lists.
    /// </summary>
    public partial class PackingPageBase
    {
        /// <summary>
        /// Whether packing this quantity would push the package past the
        /// courier's limit. Tells the operator when it would, so callers can
        /// just bail out on true.
        /// </summary>
        protected bool ExceedsWeightLimit(JlItemDto item, decimal qty)
        {
            if (!CourierHelper.AllowedCouriersForLabel.Contains(CurrentJl.Courier))
                return false;

            var afterPacking = PackedItems.Sum(w => w.ItemWeight * w.JlQuantity) + (item.ItemWeight * qty);
            if (afterPacking <= CourierConfiguration.MaxPackageWeight)
                return false;

            Toast.Show("Błąd!", $"Przekroczona waga, dopuszczalna: {CourierConfiguration.MaxPackageWeight}");
            return true;
        }

        /// <summary>The ERP call is identical for both packing modes.</summary>
        protected AddPackedPositionRequest BuildPackedPositionRequest(JlItemDto item, decimal qty) => new()
        {
            PackingDocumentId = PackageId,
            SourceDocumentId = item.DocumentId,
            SourceDocumentType = item.DocumentType,
            PositionNumber = item.ErpPositionNumber,
            StationNumber = Settings.StationNumber,
            Username = UserSession.Username,
            Quantity = qty,
            Weight = item.ItemWeight,
            Volume = item.ItemVolume,
            ScanDate = item.ScanDate,
            PackDate = DateTime.Now
        };

        /// <summary>The ERP call is identical for both packing modes.</summary>
        protected RemovePackedPositionRequest BuildRemovePositionRequest(JlItemDto packed) => new()
        {
            PackingDocumentId = PackageId,
            SourceDocumentId = packed.DocumentId,
            SourceDocumentType = packed.DocumentType,
            PositionNumber = packed.ErpPositionNumber,
            Quantity = packed.JlQuantity,
            Weight = packed.ItemWeight,
            Volume = packed.ItemVolume
        };

        protected virtual async Task<bool> MoveItemToPacked(JlItemDto item, decimal qty)
        {
            if (item == null) return false;
            if (ExceedsWeightLimit(item, qty)) return false;

            var request = BuildPackedPositionRequest(item, qty);

            var result = await CollaborationService.PackAsync(
                packageId: PackageId,
                itemCode: item.ItemCode,
                documentId: item.DocumentId,
                erpPositionNumber: item.ErpPositionNumber,
                jlCode: item.JlCode,
                qty: qty,
                packingUser: UserSession.Username,
                persistCallback: () => PackingService.AddPackedPosition(request));

            if (!result.Success || result.Snapshot == null)
            {
                Toast.Show("Błąd!", string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? "Nie udało się dodać spakowanej pozycji."
                    : result.ErrorMessage);
                return false;
            }

            await AnimateListChangeAsync(() => ApplySnapshot(result.Snapshot));
            return true;
        }

        protected virtual async Task PackAllItems()
        {
            if (JlItems == null || !JlItems.Any()) return;

            foreach (var item in JlItems.ToList())
            {
                bool moved = await MoveItemToPacked(item, item.JlQuantity);
                if (!moved) return;
            }
        }

        protected virtual async Task UnpackItem()
        {
            if (SelectedPackedItem == null) return;

            if (SelectedPackedItem.PackedWMS)
            {
                Toast.Show("Błąd!", "Nie można usunąć pozycji, która została już wysłana do WMS.", ToastType.Error, 3500);
                return;
            }

            if (!string.Equals(SelectedPackedItem.PackingUser, UserSession.Username, StringComparison.OrdinalIgnoreCase))
            {
                Toast.Show("Brak uprawnień", "Możesz usunąć tylko pozycje spakowane przez siebie.", ToastType.Error, 3500);
                return;
            }

            var request = BuildRemovePositionRequest(SelectedPackedItem);

            var result = await CollaborationService.UnpackAsync(
                packageId: PackageId,
                itemCode: SelectedPackedItem.ItemCode,
                documentId: SelectedPackedItem.DocumentId,
                erpPositionNumber: SelectedPackedItem.ErpPositionNumber,
                jlCode: SelectedPackedItem.JlCode,
                qty: SelectedPackedItem.JlQuantity,
                packingUser: SelectedPackedItem.PackingUser,
                persistCallback: () => PackingService.RemovePackedPosition(request));

            if (!result.Success || result.Snapshot == null)
            {
                Toast.Show("Błąd!", string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? "Nie udało się usunąć spakowanej pozycji."
                    : result.ErrorMessage);
                return;
            }

            await AnimateListChangeAsync(() =>
            {
                ApplySnapshot(result.Snapshot);
                SelectedPackedItem = null;
            });
        }

        protected virtual async Task HandleCodeInput(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                try
                {
                    var code = await ScanInputComponent.GetValueAsync();

                    if (!string.IsNullOrWhiteSpace(code) && JlItems != null)
                    {
                        var matches = JlItems.Where(x => MatchesScanCode(x, code)).ToList();

                        if (matches.Any())
                        {
                            var distinctJls = matches
                                .Select(x => (x.JlCode, x.JlEanCode))
                                .Distinct()
                                .ToList();
                            JlItemDto? item = null;

                            if (distinctJls.Count > 1)
                            {
                                var selectedJl = await JlSelectModal.ShowAsync(distinctJls, code);
                                if (string.IsNullOrWhiteSpace(selectedJl))
                                    return;

                                item = matches.FirstOrDefault(x => string.Equals(x.JlCode, selectedJl, StringComparison.OrdinalIgnoreCase));
                            }
                            else
                            {
                                item = matches.FirstOrDefault();
                            }

                            if (item != null)
                                OpenProductModal(item);
                        }
                        else
                        {
                            Toast.Show("Brak towaru", $"Brak towaru o kodzie {code}", ToastType.Info, 3000);
                            _ = ScanInputComponent.Shake();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Toast.Show("Błąd!", $"Błąd przy próbie zaczytania towaru: {ex.Message}");
                }
                finally
                {
                    await ScanInputComponent.ClearAsync();
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        protected virtual bool MatchesScanCode(JlItemDto item, string code)
        {
            return string.Equals(item.ItemCode, code, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(item.ItemName, code, StringComparison.OrdinalIgnoreCase) ||
                   (item.SupplierCode != null && item.SupplierCode.Any(sc => string.Equals(sc, code, StringComparison.OrdinalIgnoreCase))) ||
                   (item.ItemEan != null && item.ItemEan.Any(ean => string.Equals(ean, code, StringComparison.OrdinalIgnoreCase)));
        }

        protected virtual async void OpenProductModal(JlItemDto item)
        {
            item.ScanDate = DateTime.Now;
            await ProductSelectModal.Show(item);
            SelectedItem = item;
        }

        protected virtual void SelectPackedItem(JlItemDto packed)
        {
            SelectedPackedItem = packed;
        }

        protected virtual async Task PackItem(decimal qty)
        {
            try
            {
                if (SelectedItem == null || JlItems == null) return;

                bool moved = await MoveItemToPacked(SelectedItem, qty);
                if (!moved) return;

                // Auto finish packing only for main operator
                if (!JlItems.Any() && IsMainOperator)
                {
                    FinishPacking();
                }
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy próbie spakowania towaru: {ex.Message}");
            }
            finally
            {
                SelectedItem = null;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
