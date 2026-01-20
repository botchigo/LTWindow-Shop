using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Helpers;
using MyShop.Infrastructure;
using MyShop.Modules.Products.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Products.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CategoryManagementPage : Page
    {
        public CategoryManagementViewModel ViewModel { get; }
        public CategoryManagementPage()
        {
            InitializeComponent();
            ViewModel = ServiceHelper.GetService<CategoryManagementViewModel>();
            DataContext = ViewModel;
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.DataContext is IGetCategories_Categories_Items item)
            {
                ViewModel.DeleteCategoryCommand.Execute(item.Id);
            }
        }
    }
}
