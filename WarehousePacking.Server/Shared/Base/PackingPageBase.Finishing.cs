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
    /// Closing a package: labels, internal barcodes, buffering and handing the
    /// document over to WMS.
    /// </summary>
    public partial class PackingPageBase
    {
        protected virtual void FinishPacking()
        {
            if (!EnsureMainOperator("zakończenie pakowania"))
                return;

            _currentPackingFlow = PackingFlow.FinishPacking;
            ShowPackingModal();
        }

        protected virtual void ShowPackingModal()
        {
            try
            {
                if (!PackedItems.Any())
                {
                    Toast.Show("Błąd!", "Brak spakowanych towarów!");
                    return;
                }

                FinishPackingModal.Show();
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy próbie finalizacji pakowania: {ex.Message}");
            }
        }

        protected virtual async Task OpenPackage(DocumentStatus status = DocumentStatus.Ready)
        {
            if (CurrentJl.PackageClosed && !PackingToBufor)
            {
                var success = await PackingService.OpenPackage(PackageId);
                if (!success)
                {
                    Toast.Show("Błąd!", "Nie udało się otworzyć dokumentu pakowania. Spróbuj ponownie.");
                    return;
                }

                CurrentJl.PackageClosed = false;
                CurrentJl.InternalBarcode = string.Empty;
            }
        }

        protected virtual async Task ClosePackage(string internalBarcode, Dimensions dimensions, DocumentStatus status = DocumentStatus.Ready)
        {
            if (!CurrentJl.PackageClosed)
            {
                ClosePackageRequest closeRequest = new ClosePackageRequest
                {
                    DocumentId = PackageId,
                    InternalBarcode = internalBarcode,
                    Height = dimensions.Height,
                    Width = dimensions.Width,
                    Length = dimensions.Length,
                    Status = status,
                };

                var success = await PackingService.ClosePackage(closeRequest);
                if (!success)
                {
                    Toast.Show("Błąd!", "Nie udało się zamknąć dokumentu pakowania. Spróbuj ponownie.");
                    return;
                }

                CurrentJl.PackageClosed = true;
            }
        }

        protected virtual async Task CloseJlInWMS(string courier, string packageCode, string trackingNumber, DocumentStatus status)
        {
            if (PackedItems == null || !PackedItems.Any() || _closedWmsPackages.Contains(packageCode))
                return;

            var request = new List<WmsPackStockRequest>();

            string destinationCode = string.Empty;
            if (PackingToBufor == true)
            {
                var notClosedPackages = await PackingService.GetNotClosedPackages();
                var destinationPackage = notClosedPackages.FirstOrDefault(p => p.JlCode == packageCode || p.JlEanCode == packageCode);
                if (destinationPackage != null)
                {
                    destinationCode = destinationPackage.LocationCode;
                }
            }

            foreach (var jl in MergeJls)
            {
                var additionalPackedWmsItems = PackedItems.Where(i => i.JlCode == jl.jlName && !i.PackedWMS).Select(i => new WMSPackStockItemsRequest
                {
                    ItemCode = i.ItemCode,
                    Quantity = i.JlQuantity,
                    Packed = i.PackedWMS
                }).ToList();

                var additionalPackStockRequest = new WmsPackStockRequest
                {
                    PackingLevel = Settings.PackingLevel,
                    PackingWarehouse = Settings.PackingWarehouse,
                    DestinationCode = destinationCode,
                    LocationCode = jl.locationCode,
                    ScannedCode = packageCode,
                    TrackingNumber = trackingNumber,
                    StationNumber = Settings.StationNumber,
                    Courier = courier,
                    JlCode = jl.jlName,
                    Type = PackingToBufor ? "PALETA" : string.Empty,
                    Weight = PackedItems.Where(i => i.JlCode == jl.jlName).Sum(i => i.ItemWeight * i.JlQuantity),
                    Status = status,
                    Items = additionalPackedWmsItems
                };

                request.Add(additionalPackStockRequest);
            }

            var PackedWmsItems = PackedItems.Where(i => i.JlCode == CurrentJl.Code && !i.PackedWMS).Select(i => new WMSPackStockItemsRequest
            {
                ItemCode = i.ItemCode,
                Quantity = i.JlQuantity,
                Packed = i.PackedWMS
            }).ToList();

            var packStockRequest = new WmsPackStockRequest
            {
                PackingLevel = Settings.PackingLevel,
                PackingWarehouse = Settings.PackingWarehouse,
                LocationCode = CurrentJl.Location,
                DestinationCode = destinationCode,
                ScannedCode = packageCode,
                TrackingNumber = trackingNumber,
                StationNumber = Settings.StationNumber,
                Courier = courier,
                JlCode = CurrentJl.Code,
                Type = PackingToBufor ? "PALETA" : string.Empty,
                Weight = PackedItems.Where(i => i.JlCode == CurrentJl.Code).Sum(i => i.ItemWeight * i.JlQuantity),
                Status = status,
                Items = PackedWmsItems
            };

            request.Add(packStockRequest);
            request = request.Where(r => r.Items.Any()).ToList();

            if (request.Any())
            {
                bool isPacked = await PackingService.PackWmsStock(request);
                foreach (var item in PackedItems)
                {
                    item.PackedWMS = isPacked;
                }

                if (isPacked)
                {
                    _lastPackStockPackageId = PackageId;
                    _lastPackStockPackageCode = packageCode;
                }
                else
                {
                    Toast.Show("Błąd!", "Nie udało się spakować towaru do WMS.", ToastType.Error, 3500);
                }
            }

            if (status == DocumentStatus.Ready)
            {
                var closePackageCode = _lastPackStockPackageId == PackageId && !string.IsNullOrWhiteSpace(_lastPackStockPackageCode)
                    ? _lastPackStockPackageCode
                    : packageCode;

                if (!_closingWmsPackages.Add(closePackageCode))
                {
                    Toast.Show("Informacja", "Zamykanie paczki jest już w toku.", ToastType.Info, 3500);
                    return;
                }

                try
                {
                    var closeJlRequest = new WmsCloseJlRequest
                    {
                        PackageNumber = closePackageCode,
                        Courier = CurrentJl.Courier,
                        PackingLevel = Settings.PackingLevel,
                        PackingWarehouse = Settings.PackingWarehouse
                    };
                    await PackingService.CloseWmsJl(closeJlRequest);
                    _closedWmsPackages.Add(closePackageCode);
                }
                finally
                {
                    _closingWmsPackages.Remove(closePackageCode);
                }
            }
        }

        protected virtual void Close()
        {
            ConfirmDialog.Show(
                title: "Wyjście z kuwety",
                message: "Czy na pewno chcesz wyjść z kuwety?",
                onConfirm: async () =>
                {
                    try
                    {
                        var shouldCloseSession = IsMainOperator && !PackingToBufor;
                        await CollaborationService.LeaveSessionAsync(PackageId, UserSession.Username, shouldCloseSession);

                        // Remove the JL realization
                        await PackingService.RemoveJlRealization(CurrentJl.Code, UserSession.Username, shouldCloseSession);
                        foreach (var jl in MergeJls)
                        {
                            await PackingService.RemoveJlRealization(jl.jlName, UserSession.Username, shouldCloseSession);
                        }

                        _locationCleanupDone = true;

                        // Navigate back
                        foreach (var jl in MergeJls)
                        {
                            await PackingService.RemoveJlRealization(jl.jlName, UserSession.Username, true);
                        }
                        await PackingService.RemoveJlRealization(Jl, UserSession.Username, true);
                        Navigation.NavigateTo("/kontrola-pakowania");
                    }
                    catch (Exception ex)
                    {
                        Toast.Show("Błąd!", $"Błąd przy próbie zamknięcia pakowania: {ex.Message}");
                    }
                }
            );
        }

        protected virtual void NextPackage()
        {
            if (!EnsureMainOperator("przejście do następnej paczki"))
                return;

            if (!JlItems.Any())
            {
                Toast.Show("Błąd!", "Brak towarów do spakowania!");
                return;
            }

            _currentPackingFlow = PackingFlow.NextPackage;
            ShowPackingModal();
        }

        protected virtual async Task HandleCourierLabel()
        {
            if (!EnsureMainOperator("druk etykiety"))
                return;

            if (IsFinishingLoading)
                return;

            IsFinishingLoading = true;
            await InvokeAsync(StateHasChanged);

            try
            {
                FinishPackingModal.Hide();

                if (string.IsNullOrEmpty(CurrentJl.InternalBarcode))
                    CurrentJl.InternalBarcode = await PackingService.GenerateInternalBarcode(Settings.StationNumber);

                if (string.IsNullOrEmpty(CurrentJl.InternalBarcode))
                {
                    Toast.Show("Błąd!", "Nie udało się wygenerować numeru wewnętrznego. Spróbuj ponownie.");
                    return;
                }

                await ClosePackage(CurrentJl.InternalBarcode, new Dimensions());

                // Shipment
                var package = await ShipmentService.GetShipmentDataByBarcode(CurrentJl.InternalBarcode);

                if (package is not null && package.TaxFree)
                {
                    Toast.Show("Tax Free", "Paczka zawiera dokument Tax Free.<br/><br/><b>Nadaj numer wewnętrzny!</b>", ToastType.Info);
                    await OpenPackage();
                    return;
                }

                if (package is not null && !CurrentJl.IsCompleted && package.ShipmentServices.COD)
                {
                    Toast.Show("Nie gotowa", "Paczka nie jest jeszcze gotowa do wysyłki.<br/><br/><b>Nadaj numer wewnętrzny!</b>", ToastType.Info);
                    await OpenPackage();
                    return;
                }

                var response = await ShipmentService.SendPackage(package);

                if (response?.PackageId > 0)
                {
                    if (!string.IsNullOrEmpty(response.LabelBase64))
                    {
                        var labelContent = response.LabelBase64;
                        await ClientPrinterService.PrintAsync(Settings.PrinterLabel, response.LabelType.ToString(), labelContent);
                        ShipmentModal.Show(CurrentJl.InternalBarcode, response.LabelType, Settings.PrinterLabel, package.HasInvoice, true, null, response.TrackingNumber, response.LabelBase64);
                        if (package.HasInvoice)
                        {
                            await ClientPrinterService.PrintCrystalAsync(Settings.PrinterInvoice, package.Recipient.Country != "PL" ? "InvoiceEN" : "InvoicePL", new Dictionary<string, string> { { "DocumentId", package.InvoiceId.ToString() } });
                        }
                    }
                    else
                    {
                        var msg = string.IsNullOrWhiteSpace(response.ErrorMessage) ? "Nie udało się wygenerować etykiety kurierskiej!" : response.ErrorMessage;
                        Toast.Show("Błąd!", $"{msg}<br/><br/><b>Nadaj numer wewnętrzny!</b>");
                        await OpenPackage();
                    }
                }
                else
                {
                    var msg = string.IsNullOrWhiteSpace(response?.ErrorMessage)
                        ? "Nie udało się wygenerować przesyłki!"
                        : response.ErrorMessage;

                    Toast.Show("Błąd!", $"{msg}<br/><br/><b>Nadaj numer wewnętrzny!</b>");
                    await OpenPackage();
                }
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"{ex.Message}.<br/><br/><b>Nadaj numer wewnętrzny!</b>");
                await OpenPackage();
            }
            finally
            {
                IsFinishingLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Ends whichever flow the operator is in: back to the kuweta list, or
        /// straight into the next package. Both closing paths ran their own copy
        /// of this, differing only in whether the kuweta also leaves the packing
        /// list.
        /// </summary>
        protected async Task CompleteCurrentFlowAsync(bool removeFromPackingList)
        {
            if (_currentPackingFlow == PackingFlow.NextPackage)
            {
                await HandleNextPackage();
                return;
            }

            if (_currentPackingFlow != PackingFlow.FinishPacking)
                return;

            await CollaborationService.LeaveSessionAsync(PackageId, UserSession.Username, closeSession: true);
            await ReleaseJlsAsync(removeFromPackingList);

            Navigation.NavigateTo("/kontrola-pakowania");
        }

        /// <summary>
        /// Hands the kuweta (and anything merged into it) back. Released before
        /// navigating away, because navigation tears this component down.
        /// </summary>
        protected async Task ReleaseJlsAsync(bool removeFromPackingList)
        {
            foreach (var jlCode in MergeJls.Select(m => m.jlName).Append(Jl))
            {
                await PackingService.RemoveJlRealization(jlCode, UserSession.Username, false);

                if (removeFromPackingList)
                    await PackingService.RemoveJlFromPackingList(jlCode);
            }
        }

        protected virtual async Task HandleShipmentOkClick((string ScannedCode, string TrackingNumber) data)
        {
            if (!EnsureMainOperator("potwierdzenie wysyłki"))
                return;

            if (IsFinishingLoading)
                return;

            try
            {
                IsFinishingLoading = true;
                await InvokeAsync(StateHasChanged);

                await CloseJlInWMS(CurrentJl.CourierName, data.ScannedCode, data.TrackingNumber, DocumentStatus.Ready);

                await CompleteCurrentFlowAsync(removeFromPackingList: true);
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy próbie zwalniania jl w WMS. Spróbuj ponowanie: {ex.Message}");
                ShipmentModal.KeepOpenAfterOk();
                ShipmentModal.Show(CurrentJl.InternalBarcode, PrintDataType.ZPL, Settings.PrinterLabel, false, true, data.ScannedCode, data.TrackingNumber, string.Empty);
            }
            finally
            {
                IsFinishingLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected virtual async Task HandleInternalBarcode()
        {
            if (!EnsureMainOperator("nadanie numeru wewnętrznego"))
                return;

            if (IsFinishingLoading)
                return;

            try
            {
                FinishPackingModal.Hide();

                if (string.IsNullOrEmpty(CurrentJl.InternalBarcode))
                    CurrentJl.InternalBarcode = await PackingService.GenerateInternalBarcode(Settings.StationNumber);

                if (string.IsNullOrEmpty(CurrentJl.InternalBarcode))
                {
                    Toast.Show("Błąd!", "Nie udało się wygenerować numeru wewnętrznego. Spróbuj ponownie.");
                    return;
                }

                Dimensions dimensions = new();
                if (Settings.PackingLevel == PackingLevel.Bottom && !CourierHelper.AllowedCouriersForLabel.Contains(CurrentJl.Courier))
                {
                    dimensions = await DimensionsModal.Show();
                }

                IsFinishingLoading = true;

                await ClosePackage(CurrentJl.InternalBarcode, dimensions);
                await ClientPrinterService.PrintCrystalAsync(Settings.PrinterLabel, "Label", new Dictionary<string, string> { { "Kod Kreskowy", CurrentJl.InternalBarcode } });

                await InvokeAsync(StateHasChanged);

                ShipmentModal.Show(CurrentJl.InternalBarcode, PrintDataType.ZPL, Settings.PrinterLabel, false, true, null, null, null);
            }
            catch (Exception ex)
            {
                await OpenPackage();
                Toast.Show("Błąd!", $"Błąd przy próbie finalizacji pakowania: {ex.Message}");
            }
            finally
            {
                IsFinishingLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected virtual async Task HandleNextPackage()
        {
            try
            {
                if (!EnsureMainOperator("przygotowanie kolejnej paczki"))
                    return;

                var oldPackageId = PackageId;
                await CreatePackage(JlItems.FirstOrDefault()?.DocumentId ?? 0, JlItems.FirstOrDefault()?.DocumentType ?? 0);

                var switchResult = await CollaborationService.SwitchPackageAsync(oldPackageId, PackageId, UserSession.Username);
                if (!switchResult.Success)
                {
                    Toast.Show("Błąd!", switchResult.ErrorMessage);
                    return;
                }

                var snapshot = CollaborationService.GetSnapshot(PackageId);
                if (snapshot != null)
                {
                    ApplySnapshot(snapshot);
                }

                await UpdateRealizationsPackageId();

                var jlInProgressDto = new JlInProgressDto
                {
                    Name = Jl,
                    PackageId = PackageId
                };
                await PackingService.UpdateJlRealization(jlInProgressDto);
                CurrentJl.InternalBarcode = string.Empty;
                PackingToBufor = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy próbie przygotowania następnej paczki: {ex.Message}");
            }
        }

        protected virtual async Task HandleBufor()
        {
            if (!EnsureMainOperator("zabuforowanie paczki"))
                return;

            if (IsFinishingLoading)
                return;

            try
            {
                var password = await PasswordModal.ShowAsync("Wprowadź hasło", "Wprowadź hasło do zabuforowania paczki");
                if (password == null)
                    return;

                bool valid = await AuthService.ValidatePasswordAsync(password);
                if (!valid)
                {
                    Toast.Show("Błąd!", "Błędne hasło");
                    return;
                }

                FinishPackingModal.Hide();

                if (string.IsNullOrEmpty(CurrentJl.InternalBarcode))
                    CurrentJl.InternalBarcode = await PackingService.GenerateInternalBarcode(Settings.StationNumber);

                if (string.IsNullOrEmpty(CurrentJl.InternalBarcode))
                {
                    Toast.Show("Błąd!", "Nie udało się wygenerować numeru wewnętrznego. Spróbuj ponownie.");
                    return;
                }

                Dimensions dimensions = new();
                if (Settings.PackingLevel == PackingLevel.Bottom && !CourierHelper.AllowedCouriersForLabel.Contains(CurrentJl.Courier))
                {
                    dimensions = await DimensionsModal.Show();
                }

                IsFinishingLoading = true;
                await InvokeAsync(StateHasChanged);

                await CloseJlInWMS(CurrentJl.CourierName, CurrentJl.InternalBarcode, CurrentJl.InternalBarcode, DocumentStatus.Bufor);
                await ClosePackage(CurrentJl.InternalBarcode, dimensions, DocumentStatus.Bufor);
                await ClientPrinterService.PrintCrystalAsync(Settings.PrinterLabel, "Label", new Dictionary<string, string> { { "Kod Kreskowy", CurrentJl.InternalBarcode } });

                // Buffered packages stay on the packing list: the kuweta is
                // waiting to be topped up, unlike a package that just shipped.
                await CompleteCurrentFlowAsync(removeFromPackingList: false);
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy próbie finalizacji pakowania: {ex.Message}");
            }
            finally
            {
                IsFinishingLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
