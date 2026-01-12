using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Core.Helpers;
using MyShop.Modules.Products.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Products.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductEditorPage : Page
    {
        public ProductEditorViewModel ViewModel { get; private set; }
        public ProductEditorPage()
        {
            InitializeComponent();
            ViewModel = ServiceHelper.GetService<ProductEditorViewModel>();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.OnNavigatedTo(e.Parameter);
        }
    }
}
