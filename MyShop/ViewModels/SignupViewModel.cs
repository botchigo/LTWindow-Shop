using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MyShop.Services;

namespace MyShop.ViewModels
{
   
    public class SignupViewModel : ViewModelBase
    {
        private readonly DatabaseManager _dbManager;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _fullName = string.Empty;
        private bool _isLoading;

        public SignupViewModel(DatabaseManager dbManager)
        {
            _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));

            
            SignupCommand = new AsyncRelayCommand(SignupAsync, CanSignup);
        }

        #region Properties

      
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ((AsyncRelayCommand)SignupCommand).RaiseCanExecuteChanged();
                }
            }
        }

     
        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ((AsyncRelayCommand)SignupCommand).RaiseCanExecuteChanged();
                }
            }
        }

      
        public string FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                {
                    ((AsyncRelayCommand)SignupCommand).RaiseCanExecuteChanged();
                }
            }
        }

      
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    ((AsyncRelayCommand)SignupCommand).RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

      
        public ICommand SignupCommand { get; }

        #endregion

        #region Events

       
        public event EventHandler<SignupSuccessEventArgs>? SignupSuccess;

        
        public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        #endregion

        #region Methods

        private bool CanSignup()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(FullName);
        }

        private async Task SignupAsync()
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine($"[SignupViewModel] Starting registration for username: '{Username}'");

                
                bool canConnect = await _dbManager.UserRepository.TestConnectionAsync();
                if (!canConnect)
                {
                    RaiseError("Lỗi kết nối", "Không thế kết nối tới database.\n\nKiểm tra:\n- PostgreSQL đang chạy?\n- Thông tin kết nối đúng chưa?");
                    return;
                }

                Debug.WriteLine($"[SignupViewModel] Database connection OK");

               
                bool exists = await _dbManager.UserRepository.IsUsernameExistsAsync(Username);

                Debug.WriteLine($"[SignupViewModel] Username '{Username}' exists: {exists}");

                if (exists)
                {
                    RaiseError("Thất bại", $"Username '{Username}' đã tồn tại trong hệ thống.\nVui lòng chọn username khác.");
                    return;
                }

                Debug.WriteLine($"[SignupViewModel] Calling RegisterUserAsync...");

                bool success = await _dbManager.UserRepository.RegisterUserAsync(Username, Password, FullName);

                Debug.WriteLine($"[SignupViewModel] RegisterUserAsync returned: {success}");

                if (success)
                {
                    SignupSuccess?.Invoke(this, new SignupSuccessEventArgs
                    {
                        Username = Username,
                        FullName = FullName
                    });
                }
                else
                {
                    RaiseError("Thất bại", "Không thể đăng kí tài khoản..");
                }
            }
            catch (Npgsql.PostgresException pgEx)
            {
                Debug.WriteLine($"[SignupViewModel] PostgresException: {pgEx.Message}");

                if (pgEx.SqlState == "23505") // Unique violation
                {
                    RaiseError("Lỗi", $"Username '{Username}' đã tồn tại trong database.");
                }
                else
                {
                    RaiseError("Lỗi Database", $"PostgreSQL Error:\n{pgEx.Message}\n\nCode: {pgEx.SqlState}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignupViewModel] Exception: {ex.GetType().Name}");
                Debug.WriteLine($"[SignupViewModel] Message: {ex.Message}");
                Debug.WriteLine($"[SignupViewModel] StackTrace: {ex.StackTrace}");

                RaiseError("Lỗi", $"Đã xảy ra lỗi khi đăng ký:\n\n{ex.Message}\n\nXem Output window (View ? Output) để biết chi tiết.");
            }
            finally
            {
                IsLoading = false;
            }
        }

     
        private void RaiseError(string title, string message)
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
