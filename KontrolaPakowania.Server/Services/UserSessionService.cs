using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace KontrolaPakowania.Server.Services
{
    public class UserSessionService
    {
        private readonly IJSRuntime _js;

        public string? Username { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(Username);

        public UserSessionService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task InitializeAsync()
        {
            Username = await _js.InvokeAsync<string>("userSession.getUsername");
        }

        public async Task LoginAsync(string username)
        {
            Username = username;
            await _js.InvokeVoidAsync("userSession.setUsername", username);
        }

        public async Task LogoutAsync()
        {
            Username = null;
            await _js.InvokeVoidAsync("userSession.clearUsername");
        }
    }
}