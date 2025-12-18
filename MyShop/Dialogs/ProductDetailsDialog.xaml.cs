using Database.models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductDetailsDialog : ContentDialog
    {
        public event EventHandler RequestEdit;
        public event EventHandler RequestDelete;

        public ProductDetailsDialog(Product product)
        {
            InitializeComponent();
            DetailView.ViewModel.Product = product;
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            this.Hide();
            RequestEdit?.Invoke(this, EventArgs.Empty);
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            this.Hide();
            RequestDelete?.Invoke(this, EventArgs.Empty);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Hide(); 
        }
    }
}
