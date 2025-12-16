using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MyShop.Views;
using System;

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
            ContentFrame.Navigate(typeof(DashboardPage));
            UpdateButtonVisuals(NavDashboard);
        }

        private void Nav_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var clickedBorder = sender as Border;
            if (clickedBorder == null) return;

            string tag = clickedBorder.Tag.ToString();

            switch (tag)
            {
                case "Dashboard":
                    ContentFrame.Navigate(typeof(DashboardPage));
                    break;
                case "Products":
                    ContentFrame.Navigate(typeof(ProductManagementPage));
                    break;
                case "Orders":
                    // ContentFrame.Navigate(typeof(OrdersPage));
                    break;
                case "Statistics":
                    // ContentFrame.Navigate(typeof(StatisticsPage));
                    break;
                case "Settings":
                    // ContentFrame.Navigate(typeof(SettingsPage));
                    break;
            }

            UpdateButtonVisuals(clickedBorder);
        }
        private void UpdateButtonVisuals(Border activeBorder)
        {
            ResetButtonStyle(NavDashboard);
            ResetButtonStyle(NavProducts);
            ResetButtonStyle(NavOrders);
            // ...

            activeBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 123, 92, 214));
        }

        private void ResetButtonStyle(Border border)
        {
            border.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }

        //private void SetButtonStyle(Border border, SolidColorBrush bg, SolidColorBrush fg)
        //{
        //    if (border == null) return;
        //    border.Background = bg;

        //    if (border.Child is StackPanel sp && sp.Children.Count > 1 && sp.Children[1] is TextBlock tb)
        //    {
        //        tb.Foreground = fg;
        //        tb.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
        //    }
        //}

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Đăng xu?t",
                Content = "B?n có ch?c ch?n mu?n đăng xu?t?",
                PrimaryButtonText = "Có",
                CloseButtonText = "Không",
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            }
        }
    }
}
