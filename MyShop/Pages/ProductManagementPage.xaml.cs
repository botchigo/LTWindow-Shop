using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

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
    }
}
