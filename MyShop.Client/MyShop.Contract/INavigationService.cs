using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Contract
{
    public interface INavigationService
    {
        void NavigateTo(string pageKey, object parameter = null);
        void GoBack();
        void Configure(string key, Type pageType);
        Type GetPageType(string key);
        void SetInnerFrame(Frame frame);
    }
}
