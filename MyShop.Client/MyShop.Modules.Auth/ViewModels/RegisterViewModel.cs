using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Contract;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using StrawberryShake;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Modules.Auth.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IMyShopClient _client;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoading))]
        private bool _isLoading;

        public bool IsNotLoading => !IsLoading;

        public RegisterViewModel(
            IMyShopClient client,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _client = client;
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            //validate
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(FullName))
            {
                await _dialogService.ShowMessageAsync("Lỗi", "Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await _dialogService.ShowMessageAsync("Lỗi", "Mật khẩu nhập lại không khớp.");
                return;
            }

            //processing
            IsLoading = true;
            try
            {
                var request = new RegisterDTOInput
                {
                    Username = Username,
                    Password = Password,
                    FullName = FullName,
                };

                var result = await _client.Register.ExecuteAsync(request);

                if (result.IsErrorResult())
                {
                    var error = result.Errors.Count > 0 ? result.Errors[0].Message : "Đăng ký thất bại.";
                    await _dialogService.ShowErrorDialogAsync("Lỗi Server", error);
                }
                else if (result.Data.Register.Errors is not null)
                {
                    var error = string.Join(", ", result.Data.Register.Errors.ToList());
                    await _dialogService.ShowErrorDialogAsync("Lỗi", error);
                }
                else
                {
                    await _dialogService.ShowMessageAsync("Thành công", "Tạo tài khoản thành công! Vui lòng đăng nhập.");
                    //_navigationService.GoBack();
                    _navigationService.NavigateTo("LoginPage");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Lỗi hệ thống", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            //_navigationService.GoBack();
            _navigationService.NavigateTo("LoginPage");
        }
    }
}
