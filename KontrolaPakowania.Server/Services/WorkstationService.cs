using Blazored.LocalStorage;
using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.Server.Services
{
    public class WorkstationService
    {
        private readonly ILocalStorageService _localStorage;
        private const string StorageKey = "workstationSettings";

        public WorkstationService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<WorkstationSettings> GetSettingsAsync()
        {
            var settings = await _localStorage.GetItemAsync<WorkstationSettings>(StorageKey);
            return settings ?? new WorkstationSettings();
        }

        public async Task SaveSettingsAsync(WorkstationSettings settings)
        {
            await _localStorage.SetItemAsync(StorageKey, settings);
        }
    }
}