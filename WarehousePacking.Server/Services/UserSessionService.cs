using Microsoft.JSInterop;

namespace WarehousePacking.Server.Services
{
    public class UserSessionService
    {
        private readonly IJSRuntime _js;
        private readonly ClientContext _clientContext;

        public string? Username { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(Username);

        public UserSessionService(IJSRuntime js, ClientContext clientContext)
        {
            _js = js;
            _clientContext = clientContext;
        }

        public async Task InitializeAsync()
        {
            Username = await _js.InvokeAsync<string>("userSession.getUsername");
            _clientContext.SetUsername(Username);
        }

        public async Task LoginAsync(string username)
        {
            Username = username;
            await _js.InvokeVoidAsync("userSession.setUsername", username);
            _clientContext.SetUsername(username);
        }

        public async Task LogoutAsync()
        {
            Username = null;
            await _js.InvokeVoidAsync("userSession.clearUsername");
            _clientContext.SetUsername(null);
        }
    }
}
