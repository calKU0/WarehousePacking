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
    /// Supervisor actions dispatched from ManagerControlModal.
    /// </summary>
    public partial class PackingPageBase
    {
        protected virtual async Task OnManagerButtonClick()
        {
            var password = await PasswordModal.ShowAsync(scanInput: ScanInputComponent);
            if (password == null)
                return;

            bool valid = await AuthService.ValidatePasswordAsync(password);
            if (!valid)
            {
                Toast.Show("Błąd!", "Błędne hasło");
                await ScanInputComponent.FocusAsync();
                return;
            }

            ManagerModal.OpenModal(ScanInputComponent);
            StateHasChanged();
        }

        protected virtual async Task HandleManagerClick(int returnClick)
        {
            switch (returnClick)
            {
                //case 1: /* Etykiety */ break;
                case 2: /* Spakuj */
                    await PackAllItems();
                    break;

                //case 3: /* Zawartość */
                //    var barcode = await TextBoxModal.Show("Zawartość kuwety", "Wprowadź kod wewnętrzny", "Kod wewnętrzny");
                //    if (!string.IsNullOrEmpty(barcode))
                //    {
                //        await PackingJlItemsModal.ShowModal(barcode);
                //    }
                //    break;

                case 4: /* Kurier */
                    if (!EnsureMainOperator("zmiana kuriera"))
                        break;

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

                case 9: /* Połącz paczki */
                    try
                    {
                        var result = await MergePackagesModal.Show();
                        if (result != null)
                        {
                            var success = await PackingService.MergePackages(result);
                            if (success)
                                Toast.Show("Sukces!", $"Paczki została pomyślnie połączone. Kod kreskowy paczki:<br><br><b>{result.InitialBarcode}</b>", ToastType.Success);
                        }
                    }
                    catch (Exception ex)
                    {
                        Toast.Show("Błąd!", $"Błąd przy próbie łącznia paczek: {ex.Message}");
                    }
                    break;

                case 10: /* Do bufora */
                    try
                    {
                        var internalBarcode = await TextBoxModal.Show("Zabuforuj paczkę/palete.", "Zeskanduj kod kreskowy paczki/palety, która jest zatwierdona i nie została wygenerowana do niej wysyłka. Paczka zmieni status na bufor.", "Kod kreskowy");
                        if (string.IsNullOrEmpty(internalBarcode))
                            return;

                        var success = await PackingService.BufferPackage(internalBarcode);
                        if (!success)
                        {
                            Toast.Show("Błąd!", "Nie udało się zabuforować paczki. Spróbuj ponownie.");
                            return;
                        }

                        Toast.Show("Sukces!", "Paczka została pomyślnie zabuforowana.", ToastType.Success);
                    }
                    catch (Exception ex)
                    {
                        Toast.Show("Błąd!", $"Błąd przy próbie zabuforowania paczki: {ex.Message}");
                    }
                    break;

                case 11: /* Konfiguracja kurierów */
                    List<CourierConfiguration> courierConfigurations;
                    try
                    {
                        courierConfigurations = await PackingService.GetCourierConfiguration();
                    }
                    catch (Exception ex)
                    {
                        Toast.Show("Błąd!", $"Błąd przy próbie otwarcia konfiguracji kurierów: {ex.Message}");
                        return;
                    }

                    CourierConfigurationModal.Show(
                        configs: courierConfigurations,
                        onConfirm: async () =>
                        {
                            try
                            {
                                await PackingService.UpdateCourierConfiguration(courierConfigurations);
                                Toast.Show("Sukces", "Konfiguracja została zapisana", ToastType.Success);
                            }
                            catch (Exception ex)
                            {
                                Toast.Show("Błąd", $"Nie udało się zapisać: {ex.Message}");
                            }
                        },
                        onCancel: async () =>
                        {
                        }
                    );
                    break;

                case 12: /* Zmień hasło */
                    try
                    {
                        var newPassword = await TextBoxModal.Show("Zmień hasło kierownika", "Wprowadź nowe uniwersalne hasło kierownika do wytycznych/czerwonych kuwet itp.", "Nowe hasło", "password");
                        if (string.IsNullOrEmpty(newPassword))
                            return;

                        var success = await AuthService.ChangeManagerPasswordAsync(newPassword);
                        if (!success)
                        {
                            Toast.Show("Błąd!", "Nie udało się zmienić hasła. Spróbuj ponownie.");
                            return;
                        }

                        Toast.Show("Sukces!", "Hasło zostało zmienione.", ToastType.Success);
                    }
                    catch (Exception ex)
                    {
                        Toast.Show("Błąd!", $"Błąd przy próbie zmiany hasła: {ex.Message}");
                    }
                    break;
            }
            await ScanInputComponent.FocusAsync();
        }
    }
}
