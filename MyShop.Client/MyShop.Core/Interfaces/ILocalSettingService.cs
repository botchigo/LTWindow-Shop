namespace MyShop.Core.Interfaces
{
    public interface ILocalSettingService
    {
        Task<T?> GetSettingAsync<T>(string key);
        Task SaveSettingAsync<T>(string key, T value);
        Task RemoveSettingAsync(string key);
    }
}
