using Microsoft.UI.Xaml.Controls;
using MyShop.Contract;
using System;
using System.Collections.Generic;

namespace MyShop.Shell.Services
{
    public class NavigationService : INavigationService
    {
        private Frame? _rootFrame;
        private Frame? _innerFrame;
        private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>();

        public void SetFrame(Frame frame)
        {
            _rootFrame = frame;
        }

        public void SetInnerFrame(Frame frame)
        {
            _innerFrame = frame;
        }

        public void GoBack()
        {
            if (_innerFrame != null && _innerFrame.CanGoBack)
            {
                _innerFrame.GoBack();
                return;
            }

            if (_rootFrame == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ LỖI: Frame trong NavigationService bị Null! Kiểm tra lại DI Singleton.");
                return;
            }

            if (_rootFrame.CanGoBack)
            {
                _rootFrame.GoBack();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ History trống, chuyển hướng thẳng về Login.");
                NavigateTo("LoginPage");
            }
        }

        public void NavigateTo(string pageKey, object? parameter = null)
        {
            if (_pages.TryGetValue(pageKey, out var pageType))
            {
                //_frame?.Navigate(pageType, parameter);
                var pageInstance = Activator.CreateInstance(pageType);
                if (_rootFrame is not null)
                {
                    _rootFrame.Content = pageInstance;
                }
            }
            else
            {
                throw new ArgumentException($"Không tìm thấy trang nào có key là: {pageKey}", nameof(pageKey));
            }
        }

        public void Configure(string key, Type pageType)
        {
            if (!_pages.ContainsKey(key))
            {
                _pages.Add(key, pageType);
            }
        }

        public Type? GetPageType(string key)
        {
            if (_pages.TryGetValue(key, out var pageType))
            {
                return pageType;
            }
            return null;
        }
    }
}
