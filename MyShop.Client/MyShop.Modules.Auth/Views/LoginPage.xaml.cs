using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Helpers;
using MyShop.Modules.Auth.ViewModels;
using System;

namespace MyShop.Modules.Auth.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; }

        public LoginPage()
        {
            try
            {
                this.InitializeComponent();
                ViewModel = ServiceHelper.GetService<LoginViewModel>()
                    ?? throw new System.Exception("LoginViewModel not registered in DI");

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
    }
}
