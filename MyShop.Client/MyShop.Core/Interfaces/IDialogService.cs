namespace MyShop.Core.Interfaces
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmAsync(string title, string message);
        Task ShowMessageAsync(string title, string message);
        Task ShowErrorDialogAsync(string title, string message);
        Task<bool> ShowConfirmationDialogAsync(string title, object content, string primaryButtonText = "Đồng ý", string closeButtonText = "Hủy");
    }
}
