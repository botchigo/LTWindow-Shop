using MyShop.Core.Interfaces;
using System.Text.Json;

namespace MyShop.Infrastructure.Services
{
    public class LocalSettingService : ILocalSettingService
    {
        private const string AppName = "MyShop";
        private const string SettingsFileName = "LocalSettings.json";
        private readonly string _settingsFilePath;
        private Dictionary<string, object>? _settingsCache;

        public LocalSettingService()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(localAppData, AppName);

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _settingsFilePath = Path.Combine(appFolder, SettingsFileName);
        }

        public async Task<T?> GetSettingAsync<T>(string key)
        {
            await InitializeAsync();

            if (_settingsCache != null && _settingsCache.TryGetValue(key, out object? value))
            {
                if (value is JsonElement element)
                {
                    return element.Deserialize<T>();
                }
                return (T)value;
            }

            return default;
        }

        public async Task RemoveSettingAsync(string key)
        {
            await InitializeAsync();
            if (_settingsCache.Remove(key))
            {
                await SaveToFileAsync();
            }
        }

        public async Task SaveSettingAsync<T>(string key, T value)
        {
            await InitializeAsync();

            if (_settingsCache.ContainsKey(key))
            {
                _settingsCache[key] = value!;
            }
            else
            {
                _settingsCache.Add(key, value!);
            }

            await SaveToFileAsync();
        }

        private async Task InitializeAsync()
        {
            if (_settingsCache != null) return;

            if (!File.Exists(_settingsFilePath))
            {
                _settingsCache = new Dictionary<string, object>();
                return;
            }

            try
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                _settingsCache = JsonSerializer.Deserialize<Dictionary<string, object>>(json)
                                 ?? new Dictionary<string, object>();
            }
            catch
            {
                _settingsCache = new Dictionary<string, object>();
            }
        }

        private async Task SaveToFileAsync()
        {
            if (_settingsCache == null) return;
            string json = JsonSerializer.Serialize(_settingsCache, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
    }
}
