using Database.models;
using Database.Repositories;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Dialogs;
using System;
using System.Threading.Tasks;

namespace MyShop.Services
{
    public class DialogService : IDialogService
    {
        private readonly ICategoryRepository _categoryRepository;
        public DialogService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
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
            //dialog.CloseButtonText = "Close";
            //dialog.DefaultButton = ContentDialogButton.Close;
            //dialog.XamlRoot = mainRoot;

            dialog.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            dialog.VerticalContentAlignment = VerticalAlignment.Stretch;

            //var windowSize = mainRoot.Size;
            //dialog.MaxWidth = Math.Min(850, windowSize.Width * 0.95);
            //dialog.MaxHeight = windowSize.Height * 0.9;

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
