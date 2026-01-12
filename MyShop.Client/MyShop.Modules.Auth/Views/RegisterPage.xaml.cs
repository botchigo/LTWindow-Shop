using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Helpers;
using MyShop.Modules.Auth.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Auth.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterPage : Page
    {
        public RegisterViewModel ViewModel { get; private set; }
        public RegisterPage()
        {
            InitializeComponent();
            ViewModel = ServiceHelper.GetService<RegisterViewModel>();
            DataContext = ViewModel;
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Password = TxtPassword.Password;
            }
        }

        private void TxtConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ConfirmPassword = TxtConfirm.Password;
            }
        }
    }
}
