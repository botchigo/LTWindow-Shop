using Database.models;
using Database.Repositories;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShop.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace MyShop.Services
{
    public class DialogService : IDialogService
    {
        private readonly ICategoryRepository _categoryRepository;
        public DialogService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task ShowCategoryManagementAsync()
        {
            var content = new CategoryManagementControl();

            var dialog = new ContentDialog();
            ConfigureDialog(dialog);
            dialog.Content = content;
            dialog.CloseButtonText = "Đóng";
            dialog.DefaultButton = ContentDialogButton.Close;

            await dialog.ShowAsync();
        }

        public async Task ShowErrorDialogAsync(string title, string message)
        {
            StackPanel contentPanel = new StackPanel { Spacing = 12 };

            FontIcon warningIcon = new FontIcon
            {
                Glyph = "\uE7BA", 
                FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"],
                FontSize = 32,
                Foreground = new SolidColorBrush(Colors.OrangeRed),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock messageText = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            contentPanel.Children.Add(warningIcon);
            contentPanel.Children.Add(messageText);

            ContentDialog dialog = new ContentDialog();
            ConfigureDialog(dialog);
            dialog.Title = title;
            dialog.Content = contentPanel;
            dialog.DefaultButton = ContentDialogButton.Close;

            await dialog.ShowAsync();
        }

        public async Task<bool> ShowImportPreviewDialogAsync(List<Product> importedProducts)
        {
            var mainRoot = (App.MainWindow?.Content as UIElement)?.XamlRoot;
            if (mainRoot == null) return false;

            var previewControl = new ImportPreviewControl();
            previewControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            previewControl.VerticalAlignment = VerticalAlignment.Stretch;
            foreach (var p in importedProducts)
            {
                previewControl.ProductsImported.Add(p);
            }

            ContentDialog dialog = new ContentDialog();
            ConfigureDialog(dialog);

            if (Application.Current.Resources.TryGetValue("LargeContentDialogStyle", out object style))
            {
                dialog.Style = style as Style;
            }

            dialog.Title = $"Tìm thấy {importedProducts.Count} sản phẩm";
            dialog.Content = previewControl;
            dialog.PrimaryButtonText = "Lưu vào Database";
            dialog.CloseButtonText = "Hủy bỏ";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var result = await dialog.ShowAsync();

            return result == ContentDialogResult.Primary;
        }

        public async Task<bool> ShowProductCreateUpdateDialogAsync(Product product)
        {
            string title = product.Id == 0 ? "✨ Add new product" : $"✏️ Update: {product.Name}";

            var categories = await _categoryRepository.GetCategoriesAsync();
            var contentControl = new ProductCreateUpdateControl(product, categories);

            var dialog = new ContentDialog();
            ConfigureDialog(dialog);
            dialog.Title = "Product Details";
            dialog.PrimaryButtonText = "Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = contentControl;


            dialog.PrimaryButtonClick += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(product.Name) || product.SalePrice < 0)
                {
                    args.Cancel = true;
                    return;
                }
                contentControl.ViewModel.ApplyChanges();
            };

            var result = await dialog.ShowAsync();

            return result == ContentDialogResult.Primary;
        }

        public async Task ShowProductDetailsDialogAsync(Product product)
        {
            var mainRoot = (App.MainWindow?.Content as UIElement)?.XamlRoot;
            if (mainRoot == null)
                return;

            var dialog = new ContentDialog();

            ConfigureDialog(dialog);
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;            
            dialog.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            dialog.VerticalContentAlignment = VerticalAlignment.Stretch; 
            var viewControl = new ProductDetailsControl(product);

            dialog.Content = viewControl;
            viewControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            viewControl.VerticalAlignment = VerticalAlignment.Stretch;

            await dialog.ShowAsync();
        }

        public async Task<bool> ShowConfirmAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Confirm",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close
            };

            ConfigureDialog(dialog);

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        public async Task ShowMessageAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close
            };

            ConfigureDialog(dialog);

            await dialog.ShowAsync();
        }

        private void ConfigureDialog(ContentDialog dialog)
        {
            if (App.MainWindow?.Content is FrameworkElement rootElement)
            {
                dialog.XamlRoot = rootElement.XamlRoot;
                dialog.RequestedTheme = rootElement.RequestedTheme;
            }
        }
    }
}
