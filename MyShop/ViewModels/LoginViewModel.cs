using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MyShop.Services;

namespace MyShop.ViewModels
{
  
    public class LoginViewModel : ViewModelBase
    {
        private readonly DatabaseManager _dbManager;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _rememberMe;
        private bool _isLoading;

        public LoginViewModel()
        {
            
            _dbManager = new DatabaseManager("localhost", 5432, "MyShop", "postgres", "12345");

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
        }

        #region Properties

        
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();
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
                    ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

       
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

       
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    ((AsyncRelayCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

    
        public ICommand LoginCommand { get; }

        #endregion

        #region Events

    
        public event EventHandler<LoginSuccessEventArgs>? LoginSuccess;

    
        public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        #endregion

        #region Methods

       
        private bool CanLogin()
        {
            return !IsLoading && 
                   !string.IsNullOrWhiteSpace(Username) && 
                   !string.IsNullOrWhiteSpace(Password);
        }

      
        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine($"[LoginViewModel] Attempting login for username: '{Username}'");

                
                bool canConnect = await _dbManager.UserRepository.TestConnectionAsync();
                if (!canConnect)
                {
                    RaiseError("Lỗi kết nối", "Không thể kết nối tại database.\n\nKiểm tra:\n- PostgreSQL đang chạy?\n- Database 'MyShop' đã được tạo?");
                    return;
                }

                Debug.WriteLine($"[LoginViewModel] Database connection OK");

              
                bool success = await _dbManager.UserRepository.AuthenticateUserAsync(Username, Password);

                Debug.WriteLine($"[LoginViewModel] Authentication result: {success}");

                if (success)
                {
                   
                    var user = await _dbManager.UserRepository.GetUserByUsernameAsync(Username);
                    
                    LoginSuccess?.Invoke(this, new LoginSuccessEventArgs 
                    { 
                        Username = Username,
                        FullName = user?.FullName ?? Username
                    });
                }
                else
                {
                    RaiseError("Đăng nh?p th?t b?i", "Sai tên đăng nh?p ho?c m?t kh?u.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoginViewModel] Exception: {ex.Message}");
                RaiseError("Lỗii", $"Đã xảy ra lỗi khi đăng nhập:\n{ex.Message}");
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
