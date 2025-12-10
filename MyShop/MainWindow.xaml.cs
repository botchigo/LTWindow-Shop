using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using MyShop.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly DatabaseService _databaseService;

        public MainWindow()
        {
            InitializeComponent();
            
            // Khởi tạo DatabaseService - thay đổi thông tin kết nối theo server của bạn
            _databaseService = new DatabaseService(
                host: "localhost",
                port: 5432,
                database: "MyShop",      
                username: "postgres",     
                password: "12345" 
            );
            
            // Set window size - increased size
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(1100, 650));
            
            // Test kết nối database khi khởi động
            TestDatabaseConnection();
        }

        private async void TestDatabaseConnection()
        {
            bool isConnected = await _databaseService.TestConnectionAsync();
            if (!isConnected)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Cảnh báo",
                    Content = "Không thể kết nối tới database PostgreSQL. Vui lòng kiểm tra:\n" +
                              "1. PostgreSQL server đang chạy\n" +
                              "2. Thông tin kết nối (host, port, database, username, password)\n" +
                              "3. Database 'MyShop' đã được tạo",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Please enter your user and password.",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            try
            {
                // Xác thực người dùng từ database bảng app_user
                bool isAuthenticated = await _databaseService.AuthenticateUserAsync(username, password);
                
                if (isAuthenticated)
                {
                    ContentDialog successDialog = new ContentDialog
                    {
                        Title = "Login Success",
                        Content = $"Welcome {username}!",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await successDialog.ShowAsync();
                    
                    // TODO: Chuyển đến trang chính sau khi đăng nhập thành công
                }
                else
                {
                    ContentDialog failDialog = new ContentDialog
                    {
                        Title = "Login Fail",
                        Content = "User or PassWord Incorrect.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await failDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Can't connect to database: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            // Tạo form đăng ký
            StackPanel signupPanel = new StackPanel
            {
                Spacing = 10,
                MinWidth = 300
            };

            TextBox usernameBox = new TextBox
            {
                Header = "Username",
                PlaceholderText = "Nhập tên đăng nhập"
            };

            PasswordBox passwordBox = new PasswordBox
            {
                Header = "Password",
                PlaceholderText = "Nhập mật khẩu"
            };

            TextBox nameBox = new TextBox
            {
                Header = "Name",
                PlaceholderText = "Nhập họ tên"
            };

            signupPanel.Children.Add(usernameBox);
            signupPanel.Children.Add(passwordBox);
            signupPanel.Children.Add(nameBox);

            ContentDialog signupDialog = new ContentDialog
            {
                Title = "Đăng ký tài khoản",
                Content = signupPanel,
                PrimaryButtonText = "Đăng ký",
                CloseButtonText = "Hủy",
                XamlRoot = this.Content.XamlRoot
            };

            var result = await signupDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string username = usernameBox.Text;
                string password = passwordBox.Password;
                string name = nameBox.Text;

                // Kiểm tra thông tin nhập
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name))
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Vui lòng nhập đầy đủ thông tin.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                try
                {
                    bool isRegistered = await _databaseService.RegisterUserAsync(username, password, name);

                    if (isRegistered)
                    {
                        ContentDialog successDialog = new ContentDialog
                        {
                            Title = "Thành công",
                            Content = "Đăng ký tài khoản thành công! Bạn có thể đăng nhập ngay.",
                            CloseButtonText = "OK",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await successDialog.ShowAsync();
                    }
                    else
                    {
                        ContentDialog failDialog = new ContentDialog
                        {
                            Title = "Thất bại",
                            Content = "Username đã tồn tại. Vui lòng chọn username khác.",
                            CloseButtonText = "OK",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await failDialog.ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không thể đăng ký: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}
