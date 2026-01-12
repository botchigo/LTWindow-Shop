using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Core.Helpers;
using MyShop.Modules.Products.ViewModels;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Products.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductDetailsPage : Page
    {
        public ProductDetailsViewModel ViewModel { get; private set; }
        public ProductDetailsPage()
        {
            InitializeComponent();
            ViewModel = ServiceHelper.GetService<ProductDetailsViewModel>();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //case: from list to details
            if(e.Parameter is int productId)
            {
                ViewModel.OnNavigatedTo(productId);
            }
            //case: from edit to details
            else if(e.NavigationMode == NavigationMode.Back && ViewModel.Product is not null)
            {
                ViewModel.OnNavigatedTo(ViewModel.Product.Id);
            }
        }
    }
}
