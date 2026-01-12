using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using MyShop.Contract;
using MyShop.Core.DTOs;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure.Services;
using System;
using System.Linq;

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
                    var firstItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
                    if (firstItem != null)
                    {
                        NavView.SelectedItem = firstItem;
                        LoadPageFromTag(firstItem.Tag?.ToString());
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi Loaded: {ex}");
                }
            });
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                string? key = item.Tag?.ToString();
                if (key == "Logout")
                {
                    var userSession = App.GetService<IUserSessionService>() as UserSessionService;
                    userSession?.ClearSession();
                    _navService.NavigateTo("LoginPage");
                }
                else
                {
                    LoadPageFromTag(key);
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
    }
}
