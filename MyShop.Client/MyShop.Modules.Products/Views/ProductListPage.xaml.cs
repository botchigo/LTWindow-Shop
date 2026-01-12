using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Core.Helpers;
using MyShop.Infrastructure;
using MyShop.Modules.Products.ViewModels;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Products.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductListPage : Page
    {
        public ProductListViewModel ViewModel { get;private set; }
        public ProductListPage()
        {
            InitializeComponent();
            ViewModel = ServiceHelper.GetService<ProductListViewModel>();
            DataContext = ViewModel;
        }

        //process caching and refresh data
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(e.NavigationMode == NavigationMode.Back)
            {
                await ViewModel.RefreshDataAsync();
            }
        }

        //details click
        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is IGetProductList_Products_Items_Product item)
            {
                ViewModel.GoToDetailsCommand.Execute(item.Id);
            }
        }
    }
}
