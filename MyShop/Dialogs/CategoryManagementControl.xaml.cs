using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CategoryManagementControl : UserControl
    {
        public CategoryManagementViewModel ViewModel { get; }
        public CategoryManagementControl()
        {
            ViewModel = App.GetService<CategoryManagementViewModel>();
            InitializeComponent();
            this.Name = "Root";
        }
    }
}
