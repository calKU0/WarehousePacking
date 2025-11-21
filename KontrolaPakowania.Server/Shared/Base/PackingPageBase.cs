using KontrolaPakowania.Server.Data.Enums;
using KontrolaPakowania.Server.Services;
using KontrolaPakowania.Server.Shared.Components;
using KontrolaPakowania.Server.Shared.Components.Packing;
using KontrolaPakowania.Server.Shared.Components.Modals;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace KontrolaPakowania.Server.Shared.Base
{
    public class PackingPageBase : ComponentBase, IDisposable
    {
        [Inject] protected PackingService PackingService { get; set; } = null!;
        [Inject] protected WorkstationService WorkstationService { get; set; } = null!;
        [Inject] protected UserSessionService UserSession { get; set; } = null!;
        [Inject] protected AuthService AuthService { get; set; } = null!;
        [Inject] protected ShipmentService ShipmentService { get; set; } = null!;
        [Inject] protected ClientPrinterService ClientPrinterService { get; set; } = null!;
        [Inject] protected NavigationManager Navigation { get; set; } = null!;

        // Parameters
        [Parameter] public string Jl { get; set; } = string.Empty;

        // Shared state
        protected JlData CurrentJl = new();

        protected List<string> MergeJlsName = new();
        protected List<JlItemDto> JlItems = new();
        protected List<JlItemDto> PackedItems = new();
        protected HashSet<string> HighlightedRows = new();
        protected WorkstationSettings Settings = new();
        protected CourierConfiguration CourierConfiguration = new();
        protected int PackageId;

        // Modals & Toasts
        protected Toast Toast = new();

        protected PasswordModal ManagerPasswordModal = new();
        protected PasswordModal PackingRequirementsModal = new();
        protected ConfirmDialog ConfirmDialog = new();
        protected ManagerControlModal ManagerModal = new();
        protected TextBoxModal TextBoxModal = new();
        protected CourierModal CourierModal = new();
        protected LoggedOperatorsModal LoggedOperatorsModal = new();
        protected JlInProgressModal JlInProgressModal = new();
        protected PackingJlItemsModal PackingJlItemsModal = new();
        protected ChangePackingWarehouseModal ChangePackingWarehouseModal = new();
        protected DimensionsModal DimensionsModal = new();
        protected FinishPackingModal FinishPackingModal = new();
        protected ShipmentModal ShipmentModal = new();
        protected ScanInput ScanInputComponent = new();

        protected bool ShowProductModal = false;
        protected PasswordPurpose CurrentPasswordPurpose = PasswordPurpose.None;

        protected JlItemDto? SelectedItem;
        protected JlItemDto? SelectedPackedItem;

        protected PackingFlow _currentPackingFlow;
        protected string InternalBarcodeTemp = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await UserSession.InitializeAsync();
                if (await CheckJlNotInProgress())
                    return;
                await LoadSettings();
                LoadMergeJlsFromQuery();
                await LoadJlData();
                await AddJlRealizations();
                await ShowPackingRequirements();

                Navigation.LocationChanged += OnLocationChanged;
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy inicjalizacji: {ex.Message}");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await ScanInputComponent.FocusAsync();
            }
        }

        // ---------- Data Loading ----------
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
                CurrentJl = await PackingService.GetJlInfoByCode(Jl, Settings.PackingLevel);
                CurrentJl.InternalBarcode = InternalBarcodeTemp;
                JlItems = await PackingService.GetJlItems(CurrentJl.Name, Settings.PackingLevel);

                if (MergeJlsName.Any())
                {
                    foreach (var jl in MergeJlsName)
                    {
                        var items = await PackingService.GetJlItems(jl, Settings.PackingLevel);
                        JlItems.AddRange(items);
                    }
                }
            }
            catch (Exception ex)
            {
                JlItems = new List<JlItemDto>();
                Toast.Show("Błąd!", $"Błąd przy pobieraniu zawartości kuwety: {ex.Message}");
            }
        }

        protected virtual void LoadMergeJlsFromQuery()
        {
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

            // Read mergeNames
            if (query.TryGetValue("mergeNames", out var mergeNamesStr))
            {
                MergeJlsName = mergeNamesStr.ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            // Read packTo
            if (query.TryGetValue("packTo", out var packToValue))
            {
                PackageId = Convert.ToInt32(packToValue);
            }

            if (query.TryGetValue("barcode", out var barcodeValue))
            {
                InternalBarcodeTemp = barcodeValue;
            }
        }

        protected virtual async Task AddJlRealizations()
        {
            // Base JL realization
            await PackingService.AddJlRealization(new JlInProgressDto
            {
                Name = CurrentJl.Name,
                Courier = CurrentJl.CourierName,
                ClientName = CurrentJl.ClientName,
                StationNumber = Settings.StationNumber,
                Date = DateTime.Now,
                User = UserSession.Username
            });

            // Additional merged JLs realizations
            foreach (var jl in MergeJlsName)
            {
                await PackingService.AddJlRealization(new JlInProgressDto
                {
                    Name = jl,
                    Courier = CurrentJl.CourierName,
                    ClientName = CurrentJl.ClientName,
                    StationNumber = Settings.StationNumber,
                    Date = DateTime.Now,
                    User = UserSession.Username
                });
            }
        }

        protected virtual async Task CreatePackage()
        {
            try
            {
                var request = new CreatePackageRequest
                {
                    AddressName = CurrentJl.AddressName,
                    AddressCity = CurrentJl.AddressCity,
                    AddressStreet = CurrentJl.AddressStreet,
                    AddressPostalCode = CurrentJl.AddressPostalCode,
                    AddressCountry = CurrentJl.AddressCountry,
                    ClientId = CurrentJl.ClientId,
                    Username = UserSession.Username,
                    Courier = CurrentJl.Courier,
                    PackageWarehouse = Settings.PackingWarehouse,
                    PackingLevel = Settings.PackingLevel,
                    StationNumber = Settings.StationNumber
                };

                var packageId = await PackingService.CreatePackage(request);
                if (packageId > 0) PackageId = packageId;
                else Toast.Show("Błąd!", "Nie udało się utworzyć dokumentu pakowania.");
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy tworzeniu paczki: {ex.Message}");
            }
        }

        protected virtual async Task ShowPackingRequirements()
        {
            if (!string.IsNullOrEmpty(CurrentJl.PackingRequirements))
            {
                CurrentPasswordPurpose = PasswordPurpose.PackingRequirementAction;
                await PackingRequirementsModal.Show("Wytyczne do pakowania", CurrentJl.PackingRequirements);
            }
        }

        // ---------- Packing Logic ----------
        protected virtual async Task<bool> MoveItemToPacked(JlItemDto item, decimal qty)
        {
            if (item == null) return false;

            // Check weight limit
            if (CourierHelper.AllowedCouriersForLabel.Contains(CurrentJl.Courier) &&
                PackedItems.Sum(w => w.ItemWeight * w.JlQuantity) + (item.ItemWeight * qty) > CourierConfiguration.MaxPackageWeight)
            {
                Toast.Show("Błąd!", $"Przekroczona waga, dopuszczalna: {CourierConfiguration.MaxPackageWeight}");
                return false;
            }

            // Add to ERP package
            var request = new AddPackedPositionRequest
            {
                PackingDocumentId = PackageId,
                SourceDocumentId = item.DocumentId,
                SourceDocumentType = item.DocumentType,
                PositionNumber = item.ErpPositionNumber,
                Quantity = qty,
                Weight = item.ItemWeight,
                Volume = item.ItemVolume
            };

            var success = await PackingService.AddPackedPosition(request);
            if (!success)
            {
                Toast.Show("Błąd!", "Nie udało się dodać spakowanej pozycji.");
                return false;
            }

            // Move to PackedItems
            JlItemDto packedItem;
            var existingPacked = PackedItems.FirstOrDefault(p => p.ItemCode == item.ItemCode);
            if (qty == item.DocumentQuantity || qty == item.JlQuantity)
            {
                if (existingPacked == null)
                {
                    PackedItems.Add(item);
                    packedItem = item;
                }
                else
                {
                    existingPacked.JlQuantity += qty;
                    packedItem = existingPacked;
                }
                JlItems.Remove(item);
            }
            else
            {
                HighlightedRows.Add(item.ItemCode);
                if (existingPacked != null)
                {
                    existingPacked.JlQuantity += qty;
                    packedItem = existingPacked;
                }
                else
                {
                    packedItem = new JlItemDto
                    {
                        ItemErpId = item.ItemErpId,
                        ItemCode = item.ItemCode,
                        ItemName = item.ItemName,
                        SupplierCode = item.SupplierCode,
                        DocumentQuantity = item.DocumentQuantity,
                        JlQuantity = qty,
                        JlCode = item.JlCode,
                        ItemWeight = item.ItemWeight,
                        ItemImage = item.ItemImage,
                        ItemType = item.ItemType,
                        DestinationCountry = item.DestinationCountry,
                        ItemUnit = item.ItemUnit,
                        DocumentId = item.DocumentId
                    };
                    PackedItems.Add(packedItem);
                }

                item.JlQuantity -= qty;
                if (item.JlQuantity <= 0) JlItems.Remove(item);
            }

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

            var request = new RemovePackedPositionRequest
            {
                PackingDocumentId = PackageId,
                SourceDocumentId = SelectedPackedItem.DocumentId,
                SourceDocumentType = SelectedPackedItem.DocumentType,
                PositionNumber = SelectedPackedItem.ErpPositionNumber,
                Quantity = SelectedPackedItem.JlQuantity,
                Weight = SelectedPackedItem.ItemWeight,
                Volume = SelectedPackedItem.ItemVolume
            };

            var success = await PackingService.RemovePackedPosition(request);
            if (!success)
            {
                Toast.Show("Błąd!", "Nie udało się usunąć spakowanej pozycji.");
                return;
            }

            // --- Move back to JlItems ---
            var existingLeft = JlItems.FirstOrDefault(j => j.ItemCode == SelectedPackedItem.ItemCode);
            if (existingLeft != null)
                existingLeft.JlQuantity += SelectedPackedItem.JlQuantity;
            else
                JlItems.Add(SelectedPackedItem);

            // --- Remove from PackedItems ---
            PackedItems.Remove(SelectedPackedItem);
            HighlightedRows.Remove(SelectedPackedItem.ItemCode);

            SelectedPackedItem = null;
        }

        protected virtual async Task HandleCodeInput(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                try
                {
                    var code = await ScanInputComponent.GetValueAsync();
                    if (!string.IsNullOrEmpty(code) && JlItems != null)
                    {
                        // Search by code, barcode, or supplier code
                        var item = JlItems.FirstOrDefault(x =>
                            string.Equals(x.ItemCode, code, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(x.SupplierCode, code, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(x.ItemEan, code, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(x.ItemName, code, StringComparison.OrdinalIgnoreCase));

                        if (item != null)
                        {
                            OpenProductModal(item);
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

        protected virtual void OpenProductModal(JlItemDto item)
        {
            SelectedItem = item;
            ShowProductModal = true;
        }

        protected virtual void FinishPacking()
        {
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
                    Status = status
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

        protected virtual async Task CloseJlInWMS(string courier, string packageCode)
        {
            if (PackedItems == null || !PackedItems.Any())
                return;

            var PackedWmsItems = PackedItems.Select(i => new WMSPackStockItemsRequest
            {
                ItemCode = i.ItemCode,
                Quantity = i.JlQuantity,
            }).ToList();

            var packStockRequest = new WmsPackStockRequest
            {
                LocationCode = CurrentJl.LocationCode,
                PackageCode = packageCode,
                Courier = courier,
                JlCode = CurrentJl.Name,
                Weight = PackedItems.Sum(i => i.ItemWeight * i.JlQuantity),
                Items = PackedWmsItems
            };

            await PackingService.PackWmsStock(packStockRequest);
        }

        protected virtual async Task HandleCourierLabel()
        {
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
                    Toast.Show("Tax Free", "Paczka zawiera dokument Tax Free. Nadaj numer wewnętrzny.", ToastType.Info);
                    return;
                }
                var response = await ShipmentService.SendPackage(package);

                if (response?.PackageId > 0)
                {
                    if (!string.IsNullOrEmpty(response.LabelBase64))
                    {
                        var labelContent = response.LabelBase64;
                        await CloseJlInWMS(CurrentJl.CourierName, response.TrackingNumber);
                        await ClientPrinterService.PrintAsync(Settings.PrinterLabel, response.LabelType.ToString(), labelContent);
                        ShipmentModal.Show(response.TrackingNumber, response.LabelBase64, response.LabelType, Settings.PrinterLabel, package.HasInvoice);
                        if (package.HasInvoice)
                        {
                            await ClientPrinterService.PrintCrystalAsync(Settings.PrinterInvoice, package.RecipientCountry != "PL" ? "InvoiceEN" : "InvoicePL", new Dictionary<string, string> { { "DocumentId", package.InvoiceId.ToString() } });
                        }
                    }
                    else
                    {
                        var msg = string.IsNullOrWhiteSpace(response.ErrorMessage) ? "Nie udało się wygenerować etykiety kurierskiej!" : response.ErrorMessage;

                        Toast.Show("Błąd!", msg);
                    }
                }
                else
                {
                    var msg = string.IsNullOrWhiteSpace(response?.ErrorMessage)
                        ? "Nie udało się wygenerować przesyłki!"
                        : response.ErrorMessage;

                    Toast.Show("Błąd!", msg);
                }
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy próbie finalizacji pakowania: {ex.Message}");
            }
        }

        protected virtual async Task HandleManagerClick(int returnClick)
        {
            switch (returnClick)
            {
                case 1: /* Etykiety */ break;
                case 2: /* Spakuj */
                    await PackAllItems();
                    break;

                case 3: /* Zawartość */
                    var barcode = await TextBoxModal.Show("Zawartość kuwety", "Wprowadź kod wewnętrzny", "Kod wewnętrzny");
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        await PackingJlItemsModal.ShowModal(barcode);
                    }
                    break;

                case 4: /* Kurier */
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
                    break;

                case 5: /* Zalogowani */
                    await LoggedOperatorsModal.ShowModal();
                    break;

                case 6: /* Kuwety podjęte */
                    await JlInProgressModal.ShowModal();
                    break;

                case 7: /* Zwolnij */
                    try
                    {
                        var jlCode = await TextBoxModal.Show("Zwolnij kuwetę", "Wprowadź kod kuwety", "Kod kuwety");
                        if (!string.IsNullOrEmpty(jlCode))
                        {
                            var releaseSuccess = await PackingService.ReleaseJl(jlCode);
                            if (!releaseSuccess)
                            {
                                Toast.Show("Błąd!", "Kuweta nie została zwolniona. Spróbuj ponownie.");
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Toast.Show("Błąd!", $"Błąd przy próbie zwolnienia kuwety: {ex.Message}");
                    }
                    break;

                case 8: /* Zmień magazyn */
                    await ChangePackingWarehouseModal.Show();
                    break;
            }
            await ScanInputComponent.FocusAsync();
        }

        protected virtual async Task LoadCourierConfiguration()
        {
            try
            {
                CourierConfiguration = await PackingService.GetCourierConfiguration(CurrentJl.CourierName, Settings.PackingLevel, CurrentJl.Country);
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy pobieraniu konfiguracji kuriera: {ex.Message}");
            }
        }

        protected virtual void SelectPackedItem(JlItemDto packed)
        {
            SelectedPackedItem = packed;
        }

        protected virtual void Close()
        {
            ConfirmDialog.Show("Wyjście z kuwety", "Czy napewno chcesz wyjść z kuwety?");
        }

        protected virtual async Task HideConfirm()
        {
            ConfirmDialog.Hide();
            await ScanInputComponent.FocusAsync();
        }

        protected virtual async void ConfirmClose()
        {
            try
            {
                ConfirmDialog.Hide();
                await PackingService.RemoveJlRealization(CurrentJl.Name);
                if (PackageId > 0)
                {
                    ClosePackageRequest closePackageRequest = new()
                    {
                        DocumentId = PackageId,
                        Status = DocumentStatus.Delete
                    };
                    await PackingService.ClosePackage(closePackageRequest);
                }
                Navigation.NavigateTo("/kontrola-pakowania");
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy próbie zamknięcia pakowania: {ex.Message}");
            }
        }

        protected virtual async Task HandlePasswordConfirm(string password)
        {
            bool valid = await AuthService.ValidatePasswordAsync(password);
            if (!valid)
            {
                Toast.Show("Błąd!", "Błędne hasło");
                return;
            }

            switch (CurrentPasswordPurpose)
            {
                case PasswordPurpose.ManagerAction:
                    await ManagerPasswordModal.Hide();
                    ManagerModal.OpenModal();
                    break;

                case PasswordPurpose.PackingRequirementAction:
                    await PackingRequirementsModal.Hide();
                    break;
            }

            CurrentPasswordPurpose = PasswordPurpose.None;
        }

        protected virtual void HandlePasswordCancel()
        {
            Navigation.NavigateTo("/kontrola-pakowania");
        }

        protected virtual async Task OnManagerButtonClick()
        {
            CurrentPasswordPurpose = PasswordPurpose.ManagerAction;
            await ManagerPasswordModal.Show();
            StateHasChanged();
        }

        protected virtual void NextPackage()
        {
            _currentPackingFlow = PackingFlow.NextPackage;
            ShowPackingModal();
        }

        protected virtual async Task PackItem(decimal qty)
        {
            try
            {
                if (SelectedItem == null || JlItems == null) return;

                bool moved = await MoveItemToPacked(SelectedItem, qty);
                if (!moved) return;

                // Auto finish packing if nothing left
                if (!JlItems.Any())
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
                ShowProductModal = false;
                SelectedItem = null;
                await ScanInputComponent.FocusAsync();
                await InvokeAsync(StateHasChanged);
            }
        }

        protected virtual async Task HandleShipmentOkClick()
        {
            switch (_currentPackingFlow)
            {
                case PackingFlow.FinishPacking:
                    Navigation.NavigateTo("/kontrola-pakowania");
                    break;

                case PackingFlow.NextPackage:
                    PackedItems.Clear();
                    await CreatePackage();
                    StateHasChanged();
                    break;
            }
        }

        protected virtual async Task HandleInternalBarcode()
        {
            try
            {
                FinishPackingModal.Hide();
                string internalBarcode = string.Empty;
                if (!string.IsNullOrEmpty(CurrentJl.InternalBarcode))
                    internalBarcode = CurrentJl.InternalBarcode;
                else
                    internalBarcode = await TextBoxModal.Show("Numer wewnętrzny", "Wprowadź numer wewnętrzny", "Kod kreskowy");

                if (string.IsNullOrEmpty(internalBarcode)) return;

                Dimensions dimensions = new();
                if (Settings.PackingLevel == PackingLevel.Dół && !CourierHelper.AllowedCouriersForLabel.Contains(CurrentJl.Courier))
                {
                    dimensions = await DimensionsModal.Show();
                }

                await CloseJlInWMS(CurrentJl.CourierName, internalBarcode);
                await ClosePackage(internalBarcode, dimensions);
                await ClientPrinterService.PrintCrystalAsync(Settings.PrinterLabel, "Label", new Dictionary<string, string> { { "Kod Kreskowy", internalBarcode } });

                switch (_currentPackingFlow)
                {
                    case PackingFlow.FinishPacking:
                        Navigation.NavigateTo("/kontrola-pakowania");
                        break;

                    case PackingFlow.NextPackage:
                        PackedItems.Clear();
                        StateHasChanged();
                        break;
                }
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy próbie finalizacji pakowania: {ex.Message}");
            }
        }

        protected virtual async Task HandleBufor()
        {
            try
            {
                FinishPackingModal.Hide();

                string internalBarcode = string.Empty;
                if (!string.IsNullOrEmpty(CurrentJl.InternalBarcode))
                    internalBarcode = CurrentJl.InternalBarcode;
                else
                    internalBarcode = await TextBoxModal.Show("Numer wewnętrzny", "Wprowadź numer wewnętrzny", "Kod kreskowy");

                Dimensions dimensions = new();
                if (Settings.PackingLevel == PackingLevel.Dół && !CourierHelper.AllowedCouriersForLabel.Contains(CurrentJl.Courier))
                {
                    dimensions = await DimensionsModal.Show();
                }

                await CloseJlInWMS(CurrentJl.CourierName, internalBarcode);
                await ClosePackage(internalBarcode, dimensions, DocumentStatus.InProgress);
                await ClientPrinterService.PrintCrystalAsync(Settings.PrinterLabel, "Label", new Dictionary<string, string> { { "Kod Kreskowy", internalBarcode } });

                switch (_currentPackingFlow)
                {
                    case PackingFlow.FinishPacking:
                        Navigation.NavigateTo("/kontrola-pakowania");
                        break;

                    case PackingFlow.NextPackage:
                        PackedItems.Clear();
                        StateHasChanged();
                        break;
                }
            }
            catch (Exception ex)
            {
                Toast.Show("Błąd!", $"Błąd przy próbie finalizacji pakowania: {ex.Message}");
            }
        }

        // ---------- Lifecycle & Cleanup ----------
        protected virtual async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            var uri = Navigation.ToBaseRelativePath(Navigation.Uri);
            if (!uri.StartsWith("kontrola-pakowania/", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var jl in MergeJlsName)
                {
                    await PackingService.RemoveJlRealization(jl);
                }
                await PackingService.RemoveJlRealization(Jl);
            }
        }

        public virtual void Dispose()
        {
            Navigation.LocationChanged -= OnLocationChanged;
        }
    }
}