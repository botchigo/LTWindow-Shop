using Database.models;
using Database.Repositories;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Dialogs;
using MyShop.ViewModels;
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

        public async Task<bool> ShowProductDetailsDialogAsync(Product product)
        {
            var dialog = new ContentDialog();

            //dialog config
            ConfigureDialog(dialog);
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = product.Id == 0 ? "Add new product" : "Update product";
            dialog.PrimaryButtonText = "Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            //create content
            var categories = await _categoryRepository.GetCategoriesAsync();
            var viewModel = new ProductCategoriesViewModel()
            {
                Product = product,
                Categories = categories
            };

            var contentUsersControl = new ProductDetailsControl();
            contentUsersControl.DataContext = viewModel;

            dialog.Content = contentUsersControl;

            var result = await dialog.ShowAsync();

            return result == ContentDialogResult.Primary;
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
