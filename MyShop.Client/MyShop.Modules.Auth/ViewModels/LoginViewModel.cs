using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Contract;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using StrawberryShake;
using System;
using System.Threading.Tasks;

namespace MyShop.Modules.Auth.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IMyShopClient _client;
        private readonly IAuthenticationService _authService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _username = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe;

        [ObservableProperty]
        private bool _isLoading;

        public LoginViewModel(
            IMyShopClient client,
            IAuthenticationService authService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _client = client;
            _authService = authService;
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

        private bool CanLogin()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            IsLoading = true;
            try
            {
                var loginRequest = new LoginDTOInput
                {
                    Username = Username,
                    Password = Password,
                };

                var result = await _client.Login.ExecuteAsync(loginRequest);

                if (result.IsErrorResult())
                {
                    var error = result.Errors.Count > 0 ? result.Errors[0].Message : "Đăng nhập thất bại.";
                    await _dialogService.ShowMessageAsync("Lỗi đăng nhập", error);
                }
                else
                {
                    var data = result.Data.Login.Data;
                    await _authService.SetTokenAsync(data.AccessToken);
                    _navigationService.NavigateTo("MainLayoutPage");
                    //_navigationService.NavigateTo("DashboardPage");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi hệ thống", $"Không thể kết nối Server: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GoToRegister()
        {
            _navigationService.NavigateTo("RegisterPage");
        }
    }
}
