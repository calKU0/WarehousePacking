using WarehousePacking.Server.Services;
using Microsoft.AspNetCore.Components;

namespace WarehousePacking.Server.Shared.Base
{
    public class ProtectedPageBase : ComponentBase
    {
        [Inject] protected UserSessionService UserSession { get; set; } = null!;
        [Inject] protected WorkstationService WorkstationService { get; set; } = null!;
        [Inject] protected NavigationManager Navigation { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            var settings = await WorkstationService.GetSettingsAsync();
            if (settings is null || string.IsNullOrEmpty(settings.StationNumber))
            {
                Navigation.NavigateTo("/settings", true);
                return;
            }

            await UserSession.InitializeAsync();
            if (string.IsNullOrEmpty(UserSession.Username))
            {
                Navigation.NavigateTo("/login", true);
            }
        }
    }
}