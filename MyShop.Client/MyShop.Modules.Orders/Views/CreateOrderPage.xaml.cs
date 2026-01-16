using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Helpers;
using MyShop.Infrastructure;
using MyShop.Modules.Orders.Models;
using MyShop.Modules.Orders.ViewModels;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Orders.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateOrderPage : Page
    {
        public CreateOrderViewModel ViewModel { get; }
        public CreateOrderPage()
        {
            InitializeComponent();
            ViewModel = ServiceHelper.GetService<CreateOrderViewModel>();
            DataContext = ViewModel;

            this.Loaded += CreateOrderPage_Loaded;
        }

        private async void CreateOrderPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.SearchCommand.ExecuteAsync(null);
            ViewModel.InitPaymentMethods();
        }

        private void OnProductClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is IGetProductForOrder_Products_Items product)
            {
                if (ViewModel.AddToCartCommand.CanExecute(product))
                {
                    ViewModel.AddToCartCommand.Execute(product);
                }
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            if (button.DataContext is IGetProductForOrder_Products_Items productItem)
            {
                ViewModel.AddToCartCommand.Execute(productItem);
            }
        }

        private void SearchBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ViewModel.SearchCommand.Execute(null);
            }
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CartItem item)
            {
                ViewModel.IncreaseQuantityCommand.Execute(item);
            }
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CartItem item)
            {
                ViewModel.DecreaseQuantityCommand.Execute(item);
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CartItem item)
            {
                ViewModel.RemoveFromCartCommand.Execute(item);
            }
        }
    }
}
