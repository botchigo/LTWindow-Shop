using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services;

namespace MyShop.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly DatabaseManager _dbManager;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe;

        [ObservableProperty]
        private bool _isLoading;

        public LoginViewModel(DatabaseManager dbManager)
        {
            _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine($"[LoginViewModel] Attempting login for username: '{Username}'");

                bool canConnect = await _dbManager.UserRepository.TestConnectionAsync();
                if (!canConnect)
                {
                    OnErrorOccurred("Lỗi kết nối", "Không thể kết nối tại database.\n\nKiểm tra:\n- PostgreSQL đang chạy?\n- Database 'MyShop' đã được tạo?");
                    return;
                }

                Debug.WriteLine($"[LoginViewModel] Database connection OK");

                bool success = await _dbManager.UserRepository.AuthenticateUserAsync(Username, Password);

                Debug.WriteLine($"[LoginViewModel] Authentication result: {success}");

                if (success)
                {
                    var user = await _dbManager.UserRepository.GetUserByUsernameAsync(Username);
                    OnLoginSuccess(Username, user?.FullName ?? Username);
                }
                else
                {
                    OnErrorOccurred("Đăng nhập thất bại", "Sai tên đăng nhập hoặc mật khẩu.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoginViewModel] Exception: {ex.Message}");
                OnErrorOccurred("Lỗi", $"Đã xảy ra lỗi khi đăng nhập:\n{ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanLogin()
        {
            return !IsLoading && 
                   !string.IsNullOrWhiteSpace(Username) && 
                   !string.IsNullOrWhiteSpace(Password);
        }

        #region Events

        public event EventHandler<LoginSuccessEventArgs>? LoginSuccess;
        public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        private void OnLoginSuccess(string username, string fullName)
        {
            LoginSuccess?.Invoke(this, new LoginSuccessEventArgs 
            { 
                Username = username,
                FullName = fullName
            });
        }

        private void OnErrorOccurred(string title, string message)
        {
            ErrorOccurred?.Invoke(this, new ErrorEventArgs 
            { 
                Title = title, 
                Message = message 
            });
        }

        #endregion
    }

    #region Event Args

    public class LoginSuccessEventArgs : EventArgs
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    public class ErrorEventArgs : EventArgs
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
