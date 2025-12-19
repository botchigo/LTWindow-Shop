using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services;

namespace MyShop.ViewModels
{
    public partial class SignupViewModel : ObservableObject
    {
        private readonly DatabaseManager _dbManager;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public SignupViewModel(DatabaseManager dbManager)
        {
            _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));
        }

        [RelayCommand(CanExecute = nameof(CanSignup))]
        private async Task SignupAsync()
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine($"[SignupViewModel] Starting registration for username: '{Username}'");

                bool canConnect = await _dbManager.UserRepository.TestConnectionAsync();
                if (!canConnect)
                {
                    OnErrorOccurred("Lỗi kết nối", "Không thể kết nối tới database.\n\nKiểm tra:\n- PostgreSQL đang chạy?\n- Thông tin kết nối đúng chưa?");
                    return;
                }

                Debug.WriteLine($"[SignupViewModel] Database connection OK");

                bool exists = await _dbManager.UserRepository.IsUsernameExistsAsync(Username);

                Debug.WriteLine($"[SignupViewModel] Username '{Username}' exists: {exists}");

                if (exists)
                {
                    OnErrorOccurred("Thất bại", $"Username '{Username}' đã tồn tại trong hệ thống.\nVui lòng chọn username khác.");
                    return;
                }

                Debug.WriteLine($"[SignupViewModel] Calling RegisterUserAsync...");

                bool success = await _dbManager.UserRepository.RegisterUserAsync(Username, Password, FullName);

                Debug.WriteLine($"[SignupViewModel] RegisterUserAsync returned: {success}");

                if (success)
                {
                    OnSignupSuccess(Username, FullName);
                }
                else
                {
                    OnErrorOccurred("Thất bại", "Không thể đăng ký tài khoản.");
                }
            }
            catch (Npgsql.PostgresException pgEx)
            {
                Debug.WriteLine($"[SignupViewModel] PostgresException: {pgEx.Message}");

                if (pgEx.SqlState == "23505") // Unique violation
                {
                    OnErrorOccurred("Lỗi", $"Username '{Username}' đã tồn tại trong database.");
                }
                else
                {
                    OnErrorOccurred("Lỗi Database", $"PostgreSQL Error:\n{pgEx.Message}\n\nCode: {pgEx.SqlState}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignupViewModel] Exception: {ex.GetType().Name}");
                Debug.WriteLine($"[SignupViewModel] Message: {ex.Message}");
                Debug.WriteLine($"[SignupViewModel] StackTrace: {ex.StackTrace}");

                OnErrorOccurred("Lỗi", $"Đã xảy ra lỗi khi đăng ký:\n\n{ex.Message}\n\nXem Output window (View → Output) để biết chi tiết.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSignup()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(FullName);
        }

        #region Events

        public event EventHandler<SignupSuccessEventArgs>? SignupSuccess;
        public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        private void OnSignupSuccess(string username, string fullName)
        {
            SignupSuccess?.Invoke(this, new SignupSuccessEventArgs
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

    public class SignupSuccessEventArgs : EventArgs
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    #endregion
}
