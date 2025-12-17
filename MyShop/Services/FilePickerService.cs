using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyShop.Services
{
    public class FilePickerService : IFilePickerService
    {
        public async Task<StorageFile> PickImportFileAsync()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();

            var window = App.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

            // Filter
            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xls");
            picker.FileTypeFilter.Add(".accdb");
            picker.FileTypeFilter.Add(".mdb");

            return await picker.PickSingleFileAsync();
        }
    }
}
