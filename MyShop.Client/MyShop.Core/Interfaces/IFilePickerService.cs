using Windows.Storage;

namespace MyShop.Core.Interfaces
{
    public interface IFilePickerService
    {
        Task<IReadOnlyList<StorageFile>> PickImagesAsync();
        Task<StorageFile> PickImportFileAsync();
        Task<string> PickSaveFileAsync(string suggestedFileName, string filter);
    }
}
