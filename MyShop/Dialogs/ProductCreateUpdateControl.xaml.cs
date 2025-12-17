using Database.models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductCreateUpdateControl : UserControl
    {
        public ProductCreateUpdateViewModel ViewModel { get; }

        public ProductCreateUpdateControl(Product product, List<Category> categories)
        {           
            ViewModel = new ProductCreateUpdateViewModel(product, categories);
            InitializeComponent();
        }

        private async void OnAddImageClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            //var window = (App.Current as App).m_window;
            var window = App.MainWindow;
            var hWnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hWnd);

            var files = await picker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                var storageFolder = ApplicationData.Current.LocalFolder;

                var imagesFolder = await storageFolder.CreateFolderAsync("ProductImages", CreationCollisionOption.OpenIfExists);

                foreach (var file in files)
                {
                    string newFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
                    var copiedFile = await file.CopyAsync(imagesFolder, newFileName);

                    var newImage = new ProductImage
                    {
                        Path = copiedFile.Path,
                    };

                    ViewModel.ImageCollection.Add(newImage);

                    ViewModel.Product.ProductImages.Add(newImage);
                }
            }
        }

        private void OnRemoveImageClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ProductImage imageToRemove)
            {
                ViewModel.ImageCollection.Remove(imageToRemove);

                var itemInProduct = ViewModel.Product.ProductImages.FirstOrDefault(x => x.Path == imageToRemove.Path);
                if (itemInProduct != null)
                {
                    ViewModel.Product.ProductImages.Remove(itemInProduct);
                }
            }
        }
    }
}
