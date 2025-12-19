using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MyShop.Services;

namespace MyShop.ViewModels
{
    /// <summary>
    /// ViewModel cho màn h?nh ðãng nh?p
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private readonly DatabaseManager _dbManager;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _rememberMe;
        private bool _isLoading;

        public LoginViewModel()
        {
            // Kh?i t?o DatabaseManager
            _dbManager = new DatabaseManager("localhost", 5432, "MyShop", "postgres", "123456");

            // Kh?i t?o Commands
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
        }

        #region Properties

        /// <summary>
        /// Username ngý?i dùng nh?p
        /// </summary>
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

        /// <summary>
        /// Password ngý?i dùng nh?p
        /// </summary>
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

        /// <summary>
        /// Tr?ng thái Remember Me
        /// </summary>
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        /// <summary>
        /// Tr?ng thái ðang loading
        /// </summary>
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

        /// <summary>
        /// Command ð? ðãng nh?p
        /// </summary>
        public ICommand LoginCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event khi ðãng nh?p thành công
        /// </summary>
        public event EventHandler<LoginSuccessEventArgs>? LoginSuccess;

        /// <summary>
        /// Event khi có l?i x?y ra
        /// </summary>
        public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        #endregion

        #region Methods

        /// <summary>
        /// Ki?m tra có th? ðãng nh?p không
        /// </summary>
        private bool CanLogin()
        {
            return !IsLoading && 
                   !string.IsNullOrWhiteSpace(Username) && 
                   !string.IsNullOrWhiteSpace(Password);
        }

        /// <summary>
        /// Th?c hi?n ðãng nh?p
        /// </summary>
        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine($"[LoginViewModel] Attempting login for username: '{Username}'");

                // Ki?m tra k?t n?i database
                bool canConnect = await _dbManager.UserRepository.TestConnectionAsync();
                if (!canConnect)
                {
                    RaiseError("L?i k?t n?i", "Không th? k?t n?i t?i database.\n\nKi?m tra:\n- PostgreSQL ðang ch?y?\n- Database 'MyShop' ð? ðý?c t?o?");
                    return;
                }

                Debug.WriteLine($"[LoginViewModel] Database connection OK");

                // Xác th?c ngý?i dùng
                bool success = await _dbManager.UserRepository.AuthenticateUserAsync(Username, Password);

                Debug.WriteLine($"[LoginViewModel] Authentication result: {success}");

                if (success)
                {
                    // L?y thông tin user
                    var user = await _dbManager.UserRepository.GetUserByUsernameAsync(Username);
                    
                    if(user is not null)
                    {
                        UserSession.StartSession(user);
                    }
                    
                    LoginSuccess?.Invoke(this, new LoginSuccessEventArgs 
                    { 
                        Username = Username,
                        FullName = user?.FullName ?? Username
                    });
                }
                else
                {
                    RaiseError("Ðãng nh?p th?t b?i", "Sai tên ðãng nh?p ho?c m?t kh?u.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoginViewModel] Exception: {ex.Message}");
                RaiseError("L?i", $"Ð? x?y ra l?i khi ðãng nh?p:\n{ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Raise error event
        /// </summary>
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

    /// <summary>
    /// Event args cho ðãng nh?p thành công
    /// </summary>
    public class LoginSuccessEventArgs : EventArgs
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event args cho l?i
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
