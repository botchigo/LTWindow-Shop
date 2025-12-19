using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels;
using MyShop.Extensions;

namespace MyShop.Views
{
    public sealed partial class LoginPage : Page
    {
        private LoginViewModel? _viewModel;

        public LoginPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Get ViewModel from DI
            _viewModel = App.GetService<LoginViewModel>();

            // Subscribe to events
            _viewModel.LoginSuccess += OnLoginSuccess;
            _viewModel.ErrorOccurred += OnErrorOccurred;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Unsubscribe from events
            if (_viewModel != null)
            {
                _viewModel.LoginSuccess -= OnLoginSuccess;
                _viewModel.ErrorOccurred -= OnErrorOccurred;
            }
        }

        private void OnLoginSuccess(object? sender, LoginSuccessEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                SetLoading(false);

                if (App.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.NavigateToDashboard(e.FullName);
                }
                else
                {
                    Frame.Navigate(typeof(DashboardPage), e.FullName);
                }
            });
        }

        private async void OnErrorOccurred(object? sender, ErrorEventArgs e)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                SetLoading(false);
                await ShowMessageAsync(e.Title, e.Message);
            });
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                await ShowMessageAsync("L?i", "ViewModel chýa ðý?c kh?i t?o.");
                return;
            }

            // Get values from UI
            _viewModel.Username = UsernameTextBox.Text?.Trim() ?? "";
            _viewModel.Password = PasswordTextBox.Password ?? "";

            // Validate
            if (string.IsNullOrEmpty(_viewModel.Username) || string.IsNullOrEmpty(_viewModel.Password))
            {
                await ShowMessageAsync("L?i", "Vui l?ng nh?p ð?y ð? thông tin.");
                return;
            }

            // Show loading
            SetLoading(true);

            // Execute login command
            try
            {
                if (_viewModel.LoginCommand.CanExecute(null))
                {
                    await _viewModel.LoginCommand.ExecuteAsync(null);
                }
                else
                {
                    SetLoading(false);
                    await ShowMessageAsync("L?i", "Không th? th?c hi?n ðãng nh?p.");
                }
            }
            catch (Exception ex)
            {
                SetLoading(false);
                await ShowMessageAsync("L?i", $"Ð? x?y ra l?i: {ex.Message}");
            }
        }

        private void SetLoading(bool isLoading)
        {
            LoginButton.IsEnabled = !isLoading;
            LoadingRing.IsActive = isLoading;
            LoadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            var panel = new StackPanel { Spacing = 12, MinWidth = 320 };
            
            var userBox = new TextBox 
            { 
                Header = "Username", 
                PlaceholderText = "Nh?p tên ðãng nh?p",
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI")
            };
            
            var passBox = new PasswordBox 
            { 
                Header = "Password", 
                PlaceholderText = "Nh?p m?t kh?u",
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI")
            };
            
            var nameBox = new TextBox 
            { 
                Header = "H? tên", 
                PlaceholderText = "Nh?p h? tên",
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI")
            };

            panel.Children.Add(userBox);
            panel.Children.Add(passBox);
            panel.Children.Add(nameBox);

            var dialog = new ContentDialog
            {
                Title = "Ðãng k? tài kho?n",
                Content = panel,
                PrimaryButtonText = "Ðãng k?",
                CloseButtonText = "H?y",
                XamlRoot = this.XamlRoot,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI")
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                // Get SignupViewModel from DI
                var signupViewModel = App.GetService<SignupViewModel>();
                signupViewModel.Username = userBox.Text?.Trim() ?? "";
                signupViewModel.Password = passBox.Password ?? "";
                signupViewModel.FullName = nameBox.Text?.Trim() ?? "";

                // Subscribe to events
                signupViewModel.SignupSuccess += async (s, args) =>
                {
                    await DispatcherQueue.EnqueueAsync(async () =>
                    {
                        await ShowMessageAsync("Thành công", 
                            $"Ðãng k? thành công!\n\nUsername: {args.Username}\nH? tên: {args.FullName}\n\nB?n có th? ðãng nh?p ngay bây gi?.");
                    });
                };

                signupViewModel.ErrorOccurred += async (s, args) =>
                {
                    await DispatcherQueue.EnqueueAsync(async () =>
                    {
                        await ShowMessageAsync(args.Title, args.Message);
                    });
                };

                // Validate
                if (string.IsNullOrEmpty(signupViewModel.Username) ||
                    string.IsNullOrEmpty(signupViewModel.Password) ||
                    string.IsNullOrEmpty(signupViewModel.FullName))
                {
                    await ShowMessageAsync("L?i", "Vui l?ng nh?p ð?y ð? thông tin.");
                    return;
                }

                // Execute signup command
                if (signupViewModel.SignupCommand.CanExecute(null))
                {
                    await signupViewModel.SignupCommand.ExecuteAsync(null);
                }
            }
        }

        private async Task ShowMessageAsync(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI")
            };
            await dialog.ShowAsync();
        }
    }
}
