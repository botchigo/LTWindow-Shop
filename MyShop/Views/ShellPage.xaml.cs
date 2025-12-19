using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MyShop.Services;
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
        private bool _isPaneOpen = true;
        public ShellPage()
        {
            InitializeComponent();

            var navService = App.GetService<INavigationService>() as NavigationService;
            if(navService is not null)
            {
                navService.SetFrame(this.AppFrame);
            }
            AppFrame.Navigate(typeof(DashboardPage));
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
                    AppFrame.Navigate(typeof(DashboardPage));
                    break;
                case "Products":
                    AppFrame.Navigate(typeof(ProductManagementPage));
                    break;
                case "Orders":
                    AppFrame.Navigate(typeof(OrderManagementPage));
                    break;
                case "Reports":
                    ContentFrame.Navigate(typeof(ReportPage));
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
            ResetButtonStyle(NavReports);
            // ...

            activeBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 123, 92, 214));
        }

        private void ResetButtonStyle(Border border)
        {
            border.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }

        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            _isPaneOpen = !_isPaneOpen;

            if (_isPaneOpen)
            {
                SidebarColumn.Width = new GridLength(250);

                AppTitlePanel.Visibility = Visibility.Visible;
                MenuLabel.Visibility = Visibility.Visible;
                LblDashboard.Visibility = Visibility.Visible;
                LblProducts.Visibility = Visibility.Visible;
                LblOrders.Visibility = Visibility.Visible;
                LblSettings.Visibility = Visibility.Visible;
                LblLogout.Visibility = Visibility.Visible;
            }
            else
            {
                SidebarColumn.Width = new GridLength(70); 

                AppTitlePanel.Visibility = Visibility.Collapsed;
                MenuLabel.Visibility = Visibility.Collapsed;
                LblDashboard.Visibility = Visibility.Collapsed;
                LblProducts.Visibility = Visibility.Collapsed;
                LblOrders.Visibility = Visibility.Collapsed;
                LblSettings.Visibility = Visibility.Collapsed;
                LblLogout.Visibility = Visibility.Collapsed;
            }
        }

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
