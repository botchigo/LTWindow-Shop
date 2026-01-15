using MyShop.Core.Interfaces; // Hoặc namespace chứa ISettingsService của bạn
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MyShop.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {
        // Đường dẫn file: C:\Users\[User]\AppData\Local\MyShop\settings.json
        private readonly string _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyShop");
        private readonly string _filePath;
        
        private Dictionary<string, object> _settings;

        private const string KeyPageSize = "App_PageSize";
        private const string KeyLastPage = "App_LastPage";
        private const string KeyRememberLastPage = "App_RememberLastPage";

        public SettingsService()
        {
            _filePath = Path.Combine(_folderPath, "settings.json");
            
            // Tải settings khi khởi tạo
            _settings = LoadSettings();
        }

        // --- CÁC HÀM GET/SET ---

        public int GetPageSize()
        {
            if (_settings.TryGetValue(KeyPageSize, out var val) && val is JsonElement element)
            {
                return element.GetInt32();
            }
            // Fallback nếu lưu dạng int thuần (khi chưa qua JsonElement)
            return _settings.TryGetValue(KeyPageSize, out var intVal) ? Convert.ToInt32(intVal) : 10; 
        }

        public void SetPageSize(int size)
        {
            _settings[KeyPageSize] = size;
            SaveSettings();
        }

        public string GetLastPageKey()
        {
            if (_settings.TryGetValue(KeyLastPage, out var val))
            {
                return val.ToString() ?? "DashboardPage";
            }
            return "DashboardPage";
        }

        public void SetLastPageKey(string pageKey)
        {
            if (GetIsRememberLastPageEnabled())
            {
                _settings[KeyLastPage] = pageKey;
                SaveSettings();
            }
        }

        public bool GetIsRememberLastPageEnabled()
        {
            if (_settings.TryGetValue(KeyRememberLastPage, out var val) && val is JsonElement element)
            {
                return element.GetBoolean();
            }
            return _settings.TryGetValue(KeyRememberLastPage, out var boolVal) ? Convert.ToBoolean(boolVal) : true;
        }

        public void SetIsRememberLastPageEnabled(bool isEnabled)
        {
            _settings[KeyRememberLastPage] = isEnabled;
            SaveSettings();
        }

        // --- HÀM HỖ TRỢ ĐỌC/GHI FILE ---

        private Dictionary<string, object> LoadSettings()
        {
            try
            {
                if (!Directory.Exists(_folderPath)) Directory.CreateDirectory(_folderPath);

                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
                }
            }
            catch 
            {
                // Nếu lỗi đọc file, trả về dictionary rỗng
            }
            return new Dictionary<string, object>();
        }

        private void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // Xử lý lỗi ghi file nếu cần (VD: Full ổ cứng, không có quyền)
            }
        }
    }
}