using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Server.Helpers;
using WarehousePacking.Server.Services;
using WarehousePacking.Server.Shared.Components;
using WarehousePacking.Server.Shared.Components.Modals;

namespace WarehousePacking.Server.Shared.Base
{
    /// <summary>
    /// Loading the JL, its items, workstation settings and courier configuration,
    /// plus the checks run when the screen opens.
    /// </summary>
    public partial class PackingPageBase
    {
        protected virtual async Task LoadSettings()
        {
            try
            {
                Settings = await WorkstationService.GetSettingsAsync();
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy pobieraniu ustawień stanowiska: {ex.Message}");
            }
        }

        protected virtual async Task<bool> CheckJlNotInProgress()
        {
            try
            {
                bool inProgress = await PackingService.IsJlInProgress(Jl);
                if (inProgress)
                {
                    Toast.Show("Błąd!", "Kuweta jest już pakowana na innym stanowisku.");
                    await Task.Delay(3000);
                    Navigation.NavigateTo("/kontrola-pakowania");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd sprawdzaniu czy kuweta nie jest pakowana: {ex.Message}");
            }

            return false;
        }

        protected virtual async Task LoadJlData()
        {
            try
            {
                CurrentJl = await PackingService.GetJlInfoByCode(Jl);
                CurrentJl.InternalBarcode = InternalBarcodeTemp;
                JlItems = await PackingService.GetJlItems(CurrentJl.Code);

                if (MergeJls.Any())
                {
                    foreach (var jl in MergeJls)
                    {
                        var items = await PackingService.GetJlItems(jl.jlName);
                        JlItems.AddRange(items);
                    }
                }

                JlItems = MergeDuplicateLines(JlItems);
            }
            catch (Exception ex)
            {
                JlItems = new List<JlItemDto>();
                Toast.Show("Błąd!", $"Błąd przy pobieraniu zawartości kuwety: {ex.Message}");
            }
        }

        /// <summary>
        /// Folds rows that are the same ERP line — same item, same document, same
        /// document position — into one, summing their quantity, so a line the
        /// source split across several rows shows as a single row to pack.
        /// Order is preserved: each line keeps the place of its first occurrence.
        /// </summary>
        protected static List<JlItemDto> MergeDuplicateLines(List<JlItemDto> items)
        {
            if (items is null || items.Count < 2)
                return items ?? new List<JlItemDto>();

            var byLine = new Dictionary<(int ItemErpId, int DocumentId, int Position, string jlCode), JlItemDto>();
            var merged = new List<JlItemDto>(items.Count);

            foreach (var item in items)
            {
                var line = (item.ItemErpId, item.DocumentId, item.ErpPositionNumber, item.JlCode);

                if (byLine.TryGetValue(line, out var existing))
                {
                    existing.JlQuantity += item.JlQuantity;
                }
                else
                {
                    byLine[line] = item;
                    merged.Add(item);
                }
            }

            return merged;
        }

        protected virtual void LoadMergeJlsFromQuery()
        {
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

            if (query.TryGetValue("packTo", out var packToValue))
            {
                PackageId = Convert.ToInt32(packToValue);
            }

            if (query.TryGetValue("mergeJls", out var mergeJlsValue))
            {
                MergeJls = mergeJlsValue
                    .ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x =>
                    {
                        var parts = x.Split('|', 2);
                        return (
                            jlCode: Uri.UnescapeDataString(parts[0]),
                            location: parts.Length > 1
                                ? Uri.UnescapeDataString(parts[1])
                                : string.Empty
                        );
                    })
                    .ToList();
            }

            if (query.TryGetValue("barcode", out var barcodeValue))
            {
                InternalBarcodeTemp = barcodeValue;
                PackingToBufor = true;
            }
        }

        protected virtual async Task AddJlRealizations()
        {
            // Base JL realization
            await PackingService.AddJlRealization(new JlInProgressDto
            {
                Name = CurrentJl.Code,
                Courier = CurrentJl.Courier,
                ClientName = CurrentJl.ClientSymbol,
                StationNumber = Settings.StationNumber,
                Date = DateTime.Now,
                User = UserSession.Username,
                PackageId = PackageId
            });

            // Additional merged JLs realizations
            foreach (var jl in MergeJls)
            {
                await PackingService.AddJlRealization(new JlInProgressDto
                {
                    Name = jl.jlName,
                    Courier = CurrentJl.Courier,
                    ClientName = CurrentJl.ClientSymbol,
                    StationNumber = Settings.StationNumber,
                    Date = DateTime.Now,
                    User = UserSession.Username,
                    PackageId = PackageId
                });
            }
        }

        protected virtual async Task CreatePackage(int docId, int docType)
        {
            try
            {
                var request = new CreatePackageRequest
                {
                    ClientId = CurrentJl.ClientId,
                    DocumentId = docId,
                    DocumentType = docType,
                    Username = UserSession.Username,
                    Courier = CurrentJl.Courier,
                    PackageWarehouse = Settings.PackingWarehouse,
                    PackingLevel = Settings.PackingLevel,
                    StationNumber = Settings.StationNumber,
                    IsCompleted = CurrentJl.IsCompleted,
                    AddressId = CurrentJl.AddressId,
                    AddressType = CurrentJl.AddressType,
                };

                var packageId = await PackingService.CreatePackage(request);
                if (packageId > 0)
                {
                    PackageId = packageId;
                    CurrentJl.InternalBarcode = string.Empty;
                    CurrentJl.PackageClosed = false;
                }
                else Toast.Show("Błąd!", "Nie udało się utworzyć dokumentu pakowania.");
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy tworzeniu paczki: {ex.Message}");
            }
        }

        protected virtual async Task ShowPackingRequirements()
        {
            string packingRequirements = string.Join(". ", JlItems.Where(x => !string.IsNullOrEmpty(x.PackingRequirements)).Select(x => x.PackingRequirements).Distinct());
            if (!string.IsNullOrEmpty(packingRequirements))
            {
                var password = await PasswordModal.ShowAsync("Wytyczne do pakowania", packingRequirements);
                if (password == null || password == string.Empty)
                {
                    foreach (var jl in MergeJls)
                    {
                        await PackingService.RemoveJlRealization(jl.jlName, UserSession.Username, true);
                    }
                    await PackingService.RemoveJlRealization(Jl, UserSession.Username, true);
                    Navigation.NavigateTo("/kontrola-pakowania");
                    return;
                }

                bool valid = await AuthService.ValidatePasswordAsync(password);
                if (!valid)
                {
                    Toast.Show("Błąd!", "Błędne hasło");
                    foreach (var jl in MergeJls)
                    {
                        await PackingService.RemoveJlRealization(jl.jlName, UserSession.Username, true);
                    }
                    await PackingService.RemoveJlRealization(Jl, UserSession.Username, true);
                    Navigation.NavigateTo("/kontrola-pakowania");
                }

                StateHasChanged();
            }
        }

        protected virtual async Task CheckRouteStatus()
        {
            var routeStatuses = await ShipmentService.GetRoutesStatus();
            if (routeStatuses.DPDClosed && routeStatuses.FedexClosed && routeStatuses.GLSClosed)
                return;

            if (((routeStatuses.DPDClosed && CurrentJl.Courier == Courier.DPD)
                || (routeStatuses.FedexClosed && CurrentJl.Courier == Courier.Fedex)
                || (routeStatuses.GLSClosed && CurrentJl.Courier == Courier.GLS)) && CurrentJl.DestinationCountry == "PL")
            {
                ConfirmDialog.Show("Kurier poza trasą", $"Trasa kuriera {CurrentJl.Courier.GetDescription()} została zamknięta. Czy chcesz zmienić kuriera?",
                    onConfirm: async () =>
                    {
                        try
                        {
                            var selectedCourier = await CourierModal.ShowModal(CurrentJl.Courier);
                            if (selectedCourier.HasValue && selectedCourier.Value != Courier.Unknown)
                            {
                                UpdatePackageCourierRequest updateRequest = new UpdatePackageCourierRequest
                                {
                                    PackageId = PackageId,
                                    DocumentId = JlItems.FirstOrDefault()?.DocumentId,
                                    Courier = selectedCourier.Value
                                };

                                var success = await PackingService.UpdatePackageCourier(updateRequest);
                                if (!success)
                                {
                                    Toast.Show("Błąd!", "Nie udało się zmienić kuriera. Spróbuj ponownie.");
                                    return;
                                }

                                CurrentJl.Courier = selectedCourier.Value;
                                await LoadCourierConfiguration();
                                await InvokeAsync(StateHasChanged);
                            }
                        }
                        catch (Exception ex)
                        {
                            Toast.Show("Błąd!", $"Błąd przy próbie zmiany kuriera: {ex.Message}");
                        }
                    });
            }
        }

        protected virtual async Task LoadCourierConfiguration()
        {
            try
            {
                CourierConfiguration = (await PackingService.GetCourierConfiguration(CurrentJl.CourierName, Settings.PackingLevel, CurrentJl.DestinationCountry)).First();
                if (!string.IsNullOrEmpty(CurrentJl.InternalBarcode))
                {
                    var package = await ShipmentService.GetShipmentDataByBarcode(CurrentJl.InternalBarcode);
                    if (package != null)
                    {
                        CourierConfiguration.MaxPackageWeight -= package.Weight;
                    }
                }
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy pobieraniu konfiguracji kuriera: {ex.Message}");
            }
        }
    }
}
