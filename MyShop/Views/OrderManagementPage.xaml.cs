using Database.models;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OrderManagementPage : Page
    {
        public OrderManagementViewModel ViewModel { get; }
        public OrderManagementPage()
        {
            ViewModel = App.GetService<OrderManagementViewModel>();
            InitializeComponent();
            this.Loaded += async (s, e) => await ViewModel.LoadDataCommand.ExecuteAsync(null);
        }

        private void OrderList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Order clickedOrder)
            {
                if (ViewModel.ViewDetailsCommand.CanExecute(clickedOrder))
                {
                    ViewModel.ViewDetailsCommand.Execute(clickedOrder);
                }
            }
        }
    }
}
