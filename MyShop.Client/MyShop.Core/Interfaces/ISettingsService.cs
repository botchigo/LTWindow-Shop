namespace MyShop.Core.Interfaces
{
    public interface ISettingsService
    {
        // 1. Cấu hình phân trang (Pagination)
        int GetPageSize();
        void SetPageSize(int size);

        // 2. Cấu hình nhớ trang cuối (Last Page)
        string GetLastPageKey();
        void SetLastPageKey(string pageKey);

        // 3. Cấu hình bật/tắt tính năng nhớ trang
        bool GetIsRememberLastPageEnabled();
        void SetIsRememberLastPageEnabled(bool isEnabled);
    }
}