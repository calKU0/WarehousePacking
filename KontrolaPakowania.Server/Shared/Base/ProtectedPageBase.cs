using KontrolaPakowania.Server.Services;
using Microsoft.AspNetCore.Components;

namespace KontrolaPakowania.Server.Shared.Base
{
    public class ProtectedPageBase : ComponentBase
    {
        [Inject] protected UserSessionService UserSession { get; set; } = null!;
        [Inject] protected NavigationManager Navigation { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            await UserSession.InitializeAsync();

            if (!UserSession.IsLoggedIn)
            {
                Navigation.NavigateTo("/login", true); // redirect to login if not logged in
            }
        }
    }
}