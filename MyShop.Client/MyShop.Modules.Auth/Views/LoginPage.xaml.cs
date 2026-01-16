using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using MyShop.Core.Helpers;
using MyShop.Core.Interfaces;
using MyShop.Modules.Auth.ViewModels;
using System;
using System.Reflection;
using Windows.Storage;

namespace MyShop.Modules.Auth.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; }

        private readonly IDialogService _dialogService;

        public LoginPage()
        {
            try
            {
                this.InitializeComponent();
                ViewModel = ServiceHelper.GetService<LoginViewModel>()
                    ?? throw new System.Exception("LoginViewModel not registered in DI");

                _dialogService = ServiceHelper.GetService<IDialogService>()
                    ?? throw new System.Exception("IDialogService not registered in DI");

                LoadAppVersion();

                DataContext = ViewModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                ViewModel.Password = passwordBox.Password;
            }
        }

        private void LoadAppVersion()
        {
            Version version;
            try
            {
                var packageVer = Windows.ApplicationModel.Package.Current.Id.Version;
                version = new Version(packageVer.Major, packageVer.Minor, packageVer.Build, packageVer.Revision);
            }
            catch
            {
                version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0, 0);
            }

            AppVersionText.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";
        }

        private async void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var urlTextBox = new TextBox
            {
                Header = "Server API URL",
                PlaceholderText = "https://localhost:7210/graphql",
                Width = 350,
                Text = ConfigHelper.GetServerUrl() ?? "https://localhost:7210/graphql"
            };


            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "System Configuration",
                Content = urlTextBox,
                PrimaryButtonText = "Save & Restart", 
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string newUrl = urlTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(newUrl))
                {
                    ConfigHelper.SaveServerUrl(newUrl);
                    await _dialogService.ShowMessageAsync("Cấu hình đã được lưu", "Server URL mới đã được lưu. Hãy khởi động lại ứng dụng để áp dụng thay đổi.");
                    AppInstance.Restart("");
                }
            }
        }
    }
}
