using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Pages;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views;

namespace MyShop
{
    public sealed partial class MainWindow : Window
    {
        private readonly LoginViewModel _viewModel;
        private DatabaseManager? _dbManager;
        private Frame? _mainFrame;

        public MainWindow()
        {
            InitializeComponent();

            // Khởi tạo ViewModel
            _viewModel = new LoginViewModel();
            
            // Đăng ký events
            _viewModel.LoginSuccess += OnLoginSuccess;
            _viewModel.ErrorOccurred += OnErrorOccurred;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(1100, 650));
        }

        private bool InitDatabase()
        {
            if (_dbManager != null) return true;

            try
            {
                _dbManager = new DatabaseManager("localhost", 5432, "WindowApp_MyShop", "postgres", "23120138");
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Login Events

        private void OnLoginSuccess(object? sender, LoginSuccessEventArgs e)
        {
            // Chạy trên UI thread
            DispatcherQueue.TryEnqueue(() =>
            {
                // Khởi tạo database nếu chưa có
                if (!InitDatabase() || _dbManager == null)
                {
                    _ = ShowMessage("Lỗi", "Không thể kết nối database.");
                    return;
                }

                // Navigate to Dashboard
                NavigateToDashboard(e.FullName);
            });
        }

        private void OnErrorOccurred(object? sender, ErrorEventArgs e)
        {
            // Chạy trên UI thread
            DispatcherQueue.TryEnqueue(async () =>
            {
                await ShowMessage(e.Title, e.Message);
            });
        }

        #endregion

        #region Navigation

        private void NavigateToDashboard(string userName)
        {
            try
            {
                // Resize window for dashboard
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                appWindow.Resize(new Windows.Graphics.SizeInt32(1500, 900));

                // Create frame
                _mainFrame = new Frame();

                // Navigate to DashboardPage with parameters
                //_mainFrame.Navigate(typeof(DashboardPage), (_dbManager!, userName));
                _mainFrame.Navigate(typeof(ShellPage));

                // Set frame as window content
                this.Content = _mainFrame;

                // Update window title
                this.Title = "MyShop - Dashboard";
            }
            catch (Exception ex)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    await ShowMessage("Lỗi", $"Không thể mở Dashboard:\n{ex.Message}");
                });
            }
        }

        #endregion

        #region Event Handlers

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Update ViewModel properties từ UI
            _viewModel.Username = UsernameTextBox.Text?.Trim() ?? "";
            _viewModel.Password = PasswordTextBox.Password ?? "";

            // Validate
            if (string.IsNullOrEmpty(_viewModel.Username) || string.IsNullOrEmpty(_viewModel.Password))
            {
                await ShowMessage("Lỗi", "Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            // Execute login command
            if (_viewModel.LoginCommand.CanExecute(null))
            {
                _viewModel.LoginCommand.Execute(null);
            }
        }

        private async void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InitDatabase() || _dbManager == null)
            {
                await ShowMessage("Lỗi", "Không thể kết nối database.\n\nKiểm tra:\n- PostgreSQL đang chạy?\n- Database 'MyShop' đã được tạo?");
                return;
            }

            var panel = new StackPanel { Spacing = 10, MinWidth = 300 };
            var userBox = new TextBox { Header = "Username", PlaceholderText = "Nhập tên đăng nhập" };
            var passBox = new PasswordBox { Header = "Password", PlaceholderText = "Nhập mật khẩu" };
            var nameBox = new TextBox { Header = "Họ tên", PlaceholderText = "Nhập họ tên" };

            panel.Children.Add(userBox);
            panel.Children.Add(passBox);
            panel.Children.Add(nameBox);

            var dialog = new ContentDialog
            {
                Title = "Đăng ký",
                Content = panel,
                PrimaryButtonText = "Đăng ký",
                CloseButtonText = "Hủy",
                XamlRoot = this.Content.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                // Tạo SignupViewModel
                var signupViewModel = new SignupViewModel(_dbManager)
                {
                    Username = userBox.Text?.Trim() ?? "",
                    Password = passBox.Password ?? "",
                    FullName = nameBox.Text?.Trim() ?? ""
                };

                // Đăng ký events
                signupViewModel.SignupSuccess += async (s, args) =>
                {
                    await DispatcherQueue.EnqueueAsync(async () =>
                    {
                        await ShowMessage("Thành công", $"Đăng ký thành công!\n\nUsername: {args.Username}\nHọ tên: {args.FullName}\n\nBạn có thể đăng nhập ngay bây giờ.");
                    });
                };

                signupViewModel.ErrorOccurred += async (s, args) =>
                {
                    await DispatcherQueue.EnqueueAsync(async () =>
                    {
                        await ShowMessage(args.Title, args.Message);
                    });
                };

                // Validate
                if (string.IsNullOrEmpty(signupViewModel.Username) || 
                    string.IsNullOrEmpty(signupViewModel.Password) || 
                    string.IsNullOrEmpty(signupViewModel.FullName))
                {
                    await ShowMessage("Lỗi", "Vui lòng nhập đầy đủ thông tin.");
                    return;
                }

                // Execute signup command
                if (signupViewModel.SignupCommand.CanExecute(null))
                {
                    signupViewModel.SignupCommand.Execute(null);
                }
            }
        }

        #endregion

        #region Helper Methods

        private async Task ShowMessage(string title, string content)
        {
            await new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            }.ShowAsync();
        }

        #endregion
    }

    #region DispatcherQueue Extensions

    public static class DispatcherQueueExtensions
    {
        public static Task EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Func<Task> function)
        {
            var tcs = new TaskCompletionSource();
            
            dispatcher.TryEnqueue(async () =>
            {
                try
                {
                    await function();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            
            return tcs.Task;
        }
    }

    #endregion
}
