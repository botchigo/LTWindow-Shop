using System.Text.Json;

namespace MyShop.Core.Helpers
{
    public static class ConfigHelper
    {
        private static string GetConfigFilePath()
        {
            // Lấy đường dẫn: C:\Users\User\AppData\Local\MyShop\server_config.json
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyWindowProject");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return Path.Combine(folder, "server_config.json");
        }

        // Lưu URL
        public static void SaveServerUrl(string url)
        {
            var config = new { BaseUrl = url };
            string json = JsonSerializer.Serialize(config);
            File.WriteAllText(GetConfigFilePath(), json);
        }

        // Đọc URL (trả về null nếu chưa cấu hình)
        public static string? GetServerUrl()
        {
            try
            {
                string path = GetConfigFilePath();
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("BaseUrl", out var element))
                    {
                        return element.GetString();
                    }
                }
            }
            catch
            {
                // Nếu lỗi đọc file, coi như chưa cấu hình
            }
            return null;
        }
    }
}
