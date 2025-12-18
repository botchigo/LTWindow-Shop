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
    public sealed partial class CreateOrderPage : Page
    {
        public CreateOrderViewModel ViewModel { get; }
        public CreateOrderPage()
        {
            ViewModel = App.GetService<CreateOrderViewModel>();
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        private void OnProductClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Product product)
            {
                ViewModel.AddToCartCommand.Execute(product);
            }
        }
    }
}
