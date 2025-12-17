using Database.models;
using Database.Repositories;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Dialogs;
using MyShop.Services;
using MyShop.ViewModels;
using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductManagementPage : Page
    {
        public ProductManagementViewModel ViewModel { get; }

        public ProductManagementPage()
        {
            ViewModel = App.GetService<ProductManagementViewModel>();
            InitializeComponent();
            this.Loaded += async (s, e) => await ViewModel.LoadInitalDataAndFilterAsync();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.DataContext is Product product)
            {
                ViewModel.DeleteProductCommand.Execute(product);
            }
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.DataContext is Product product)
            {
                ViewModel.UpdateProductCommand.Execute(product);
            }
        }

        private void OnViewClick(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn && btn.DataContext is Product product)
            {
                ViewModel.ViewProductCommand.Execute(product);
            }
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Product product)
            {
                if (ViewModel.ViewProductCommand.CanExecute(product))
                {
                    ViewModel.ViewProductCommand.Execute(product);
                }
            }
        }
    }
}
