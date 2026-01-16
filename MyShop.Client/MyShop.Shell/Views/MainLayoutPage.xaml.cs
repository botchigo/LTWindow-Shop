using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using MyShop.Contract;
using MyShop.Core.DTOs;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure.Authentication;
using MyShop.Infrastructure.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Shell.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainLayoutPage : Page
    {
        private readonly INavigationService _navService;
        public MainLayoutPage()
        {
            InitializeComponent();

            _navService = App.GetService<INavigationService>();

            WeakReferenceMessenger.Default.Register<NavigateInnerPageMessage>(this, (r, m) =>
            {
                NavigateToInnerPage(m.Key, m.Parameter);
            });

            this.Loaded += (s, e) =>
            {
                _navService.SetInnerFrame(ContentFrame);
            };

            this.Unloaded += (s, e) =>
            {
                _navService.SetInnerFrame(null!);
            };
        }

        private void NavigateToInnerPage(string key, object? parameter)
        {
            Type? pageType = _navService.GetPageType(key);
            if (pageType != null)
            {
                try
                {
                    var pageInstance = Activator.CreateInstance(pageType);

                    if (pageInstance is Microsoft.UI.Xaml.Controls.Page page
                        && page.DataContext is INavigationAware viewModel)
                    {
                        viewModel.OnNavigatedTo(parameter!);
                    }

                    ContentFrame.Content = pageInstance;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi Navigate chi tiết: {ex}");
                }
            }
        }

        private void NavView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    var settingsService = App.GetService<ISettingsService>();

                    // 1. Lấy trang cuối cùng đã lưu
                    // (Hàm GetLastPageKey trong Service nên trả về "DashboardPage" nếu chưa có gì)
                    string lastPageKey = settingsService?.GetLastPageKey() ?? "";
                    bool isRememberEnabled = settingsService?.GetIsRememberLastPageEnabled() ?? true;

                    NavigationViewItem? itemToSelect = null;

                    // 2. Nếu bật tính năng nhớ -> Tìm item tương ứng
                    if (isRememberEnabled && !string.IsNullOrEmpty(lastPageKey))
                    {
                        // Nếu lần trước ở trang Settings
                        if (lastPageKey == "SettingsPage")
                        {
                            NavView.SelectedItem = NavView.SettingsItem;
                            //_navService.NavigateTo("SettingsPage");
                            LoadPageFromTag("SettingsPage");
                            return;
                        }

                        itemToSelect = FindNavItemByTag(lastPageKey);
                    }

                    // 3. Fallback: Nếu không tìm thấy hoặc tắt tính năng -> Lấy item đầu tiên
                    if (itemToSelect == null)
                    {
                        itemToSelect = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
                    }

                    // 4. Thực hiện chọn và load
                    if (itemToSelect != null)
                    {
                        NavView.SelectedItem = itemToSelect;
                        LoadPageFromTag(itemToSelect.Tag?.ToString());
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi Loaded: {ex}");
                }
            });
        }

        private async void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var settingsService = App.GetService<ISettingsService>();

            //1.Xử lý khi bấm nút Bánh răng(Settings mặc định)
            if (args.IsSettingsSelected)
            {
                // Điều hướng đến trang Settings (key phải khớp với map trong NavigationService)
                LoadPageFromTag("SettingsPage"); // Hoặc _navService.NavigateTo("SettingsPage");

                // Lưu lại trạng thái
                settingsService?.SetLastPageKey("SettingsPage");
                return;
            }

            if (args.SelectedItem is NavigationViewItem item)
            {
                string? key = item.Tag?.ToString();
                if (key == "Logout")
                {
                    var authService = App.GetService<IAuthenticationService>() as AuthenticationService;
                    if (authService != null)
                    {
                        await authService.LogoutAsync();
                    }
                    _navService.NavigateTo("LoginPage");
                }
                else
                {
                    LoadPageFromTag(key);

                    // 2. [MỚI] Lưu lại trang hiện tại vào LocalSettings
                    if (!string.IsNullOrEmpty(key))
                    {
                        settingsService?.SetLastPageKey(key);
                    }
                }
            }
        }

        private void LoadPageFromTag(string? key)
        {
            if (string.IsNullOrEmpty(key)) return;

            Type? pageType = _navService.GetPageType(key);

            if (pageType != null)
            {
                try
                {
                    var pageInstance = Activator.CreateInstance(pageType);

                    ContentFrame.Content = pageInstance;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi tạo trang con: {ex}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"---> CẢNH BÁO: Không tìm thấy Page Type cho key '{key}'. Hãy kiểm tra lại MapNavigation trong Module.");
            }
        }
        private NavigationViewItem? FindNavItemByTag(string tag)
        {
            // Tìm trong Menu chính
            foreach (var item in NavView.MenuItems.OfType<NavigationViewItem>())
            {
                if (item.Tag?.ToString() == tag) return item;
            }

            // Tìm trong Footer (nếu bạn để nút Logout hay User ở dưới)
            foreach (var item in NavView.FooterMenuItems.OfType<NavigationViewItem>())
            {
                if (item.Tag?.ToString() == tag) return item;
            }

            return null;
        }
    }
}
