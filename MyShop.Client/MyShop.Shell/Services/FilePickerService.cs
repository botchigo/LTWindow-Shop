using MyShop.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace MyShop.Shell.Services
{
    public class FilePickerService : IFilePickerService
    {
        public async Task<IReadOnlyList<StorageFile>> PickImagesAsync()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var window = App.MainWindow;
            var hWnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hWnd);

            var files = await picker.PickMultipleFilesAsync();
            return files;
        }

        public async Task<StorageFile?> PickImportFileAsync()
        {
            var picker = new FileOpenPicker();

            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xls");

            picker.FileTypeFilter.Add(".accdb");
            picker.FileTypeFilter.Add(".mdb");

            var window = App.MainWindow;
            var hWnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hWnd);

            // Mở Picker
            var file = await picker.PickSingleFileAsync();
            return file;
        }
        public async Task<string> PickSaveFileAsync(string suggestedFileName, string filter)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();

            // Cần lấy Window Handle (HWND) vì WinUI 3 chạy Desktop
            var window = App.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("PDF Document", new List<string>() { ".pdf" });
            savePicker.SuggestedFileName = suggestedFileName;

            var file = await savePicker.PickSaveFileAsync();
            return file?.Path ?? string.Empty;
        }
    }
}
