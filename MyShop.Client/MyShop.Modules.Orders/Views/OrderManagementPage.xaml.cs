using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Core.Helpers;
using MyShop.Infrastructure;
using MyShop.Modules.Orders.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Orders.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OrderManagementPage : Page
    {
        public OrderManagementViewModel ViewModel { get; }
        public OrderManagementPage()
        {
            InitializeComponent();
            ViewModel = ServiceHelper.GetService<OrderManagementViewModel>();
            DataContext = ViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.Back)
            {
                if (ViewModel.LoadDataCommand.CanExecute(null))
                {
                    await ViewModel.LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        private void OrderList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is IGetOrders_Orders_Items clickedOrder)
            {
                if (ViewModel.GoToDetailsPageCommand.CanExecute(clickedOrder.Id))
                {
                    ViewModel.GoToDetailsPageCommand.Execute(clickedOrder.Id);
                }
            }
        }
    }
}
