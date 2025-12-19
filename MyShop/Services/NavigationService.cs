using Microsoft.UI.Xaml.Controls;

namespace MyShop.Services
{
    public class NavigationService : INavigationService
    {
        private Frame _frame;

        public void SetFrame(Frame frame)
        {
            this._frame = frame;
        }

        public void GoBack()
        {
            if(_frame is not null && _frame.CanGoBack)
            {
                _frame.GoBack();
            }
        }

        public void NavigateTo<T>(object? parameter = null)
        {
            if(_frame is not null)
            {
                _frame.Navigate(typeof(T), parameter);
            }
        }
    }
}
