using Blazored.LocalStorage;
using WarehousePacking.Server.Settings;

namespace WarehousePacking.Server.Services
{
    public class WorkstationService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ClientContext _clientContext;
        private const string StorageKey = "workstationSettings";

        public WorkstationService(ILocalStorageService localStorage, ClientContext clientContext)
        {
            _localStorage = localStorage;
            _clientContext = clientContext;
        }

        public async Task<WorkstationSettings> GetSettingsAsync()
        {
            var settings = await _localStorage.GetItemAsync<WorkstationSettings>(StorageKey) ?? new WorkstationSettings();

            // Keep the station identity attached to outgoing API calls current.
            _clientContext.SetStation(settings.StationNumber);

            return settings;
        }

        public async Task SaveSettingsAsync(WorkstationSettings settings)
        {
            await _localStorage.SetItemAsync(StorageKey, settings);
            _clientContext.SetStation(settings.StationNumber);
        }
    }
}
