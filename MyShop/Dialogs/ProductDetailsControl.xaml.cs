using Database.models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductDetailsControl : UserControl
    {
        public ProductDetailsViewModel ViewModel { get; set; }
        public ProductDetailsControl(Product product)
        {
            ViewModel = new ProductDetailsViewModel(product);
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = this;
            while (parent != null)
            {
                if (parent is ContentDialog dialog)
                {
                    dialog.Hide(); 
                    break;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }
    }
}
