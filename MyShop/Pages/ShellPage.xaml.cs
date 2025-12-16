using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            InitializeComponent();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            // ContentFrame.Navigate(typeof(HomePage)); 
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                // ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var item = args.InvokedItemContainer as NavigationViewItem;
                if (item != null)
                {
                    switch (item.Tag.ToString())
                    {
                        //case "HomePage":
                        //    // ContentFrame.Navigate(typeof(HomePage));
                        //    break;
                        case "ProductManagementPage":
                            ContentFrame.Navigate(typeof(ProductManagementPage));
                            break;
                        //case "Logout":
                        //    // Xử lý đăng xuất: Gọi lại MainWindow để load lại Login
                        //    (App.Current as App).m_window.Content = new MainWindow(); // Hoặc cách xử lý reset khác
                        //    break;
                    }
                }
            }
        }
    }
}
