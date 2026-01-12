using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Helpers;
using MyShop.Modules.Orders.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Orders.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OrderDetailsPage : Page
    {
        public OrderDetailsViewModel ViewModel { get; }
        public OrderDetailsPage()
        {
            InitializeComponent();
            ViewModel = ServiceHelper.GetService<OrderDetailsViewModel>();
            DataContext = ViewModel;
        }
    }
}
