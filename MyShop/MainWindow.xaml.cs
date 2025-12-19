using System;
using Microsoft.UI.Xaml;
using MyShop.Views;

namespace MyShop
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set window size
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(1100, 650));

            // Navigate to LoginPage on startup
            RootFrame.Navigate(typeof(LoginPage));
        }

        public void NavigateToDashboard(string userName)
        {
            // Resize window for dashboard
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(1500, 900));

            // Navigate to Dashboard
            RootFrame.Navigate(typeof(DashboardPage), userName);

            // Update window title
            this.Title = "MyShop - Dashboard";
        }

        public void NavigateToLogin()
        {
            // Resize window back to login size
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(1100, 650));

            // Navigate back to Login
            RootFrame.Navigate(typeof(LoginPage));

            // Update window title
            this.Title = "MyShop - Login";
        }
    }
}
