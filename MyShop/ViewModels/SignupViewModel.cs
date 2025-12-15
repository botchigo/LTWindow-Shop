using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MyShop.Services;

namespace MyShop.ViewModels
{
    /// <summary>
    /// ViewModel cho form ðãng k?
    /// </summary>
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

            // Kh?i t?o Commands
            SignupCommand = new AsyncRelayCommand(SignupAsync, CanSignup);
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
                    ((AsyncRelayCommand)SignupCommand).RaiseCanExecuteChanged();
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
                    ((AsyncRelayCommand)SignupCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// H? tên ngý?i dùng
        /// </summary>
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
                    ((AsyncRelayCommand)SignupCommand).RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command ð? ðãng k?
        /// </summary>
        public ICommand SignupCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event khi ðãng k? thành công
        /// </summary>
        public event EventHandler<SignupSuccessEventArgs>? SignupSuccess;

        /// <summary>
        /// Event khi có l?i x?y ra
        /// </summary>
        public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        #endregion

        #region Methods

        /// <summary>
        /// Ki?m tra có th? ðãng k? không
        /// </summary>
        private bool CanSignup()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(FullName);
        }

        /// <summary>
        /// Th?c hi?n ðãng k?
        /// </summary>
        private async Task SignupAsync()
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine($"[SignupViewModel] Starting registration for username: '{Username}'");

                // Ki?m tra k?t n?i database
                bool canConnect = await _dbManager.UserRepository.TestConnectionAsync();
                if (!canConnect)
                {
                    RaiseError("L?i k?t n?i", "Không th? k?t n?i t?i database.\n\nKi?m tra:\n- PostgreSQL ðang ch?y?\n- Thông tin k?t n?i ðúng chýa?");
                    return;
                }

                Debug.WriteLine($"[SignupViewModel] Database connection OK");

                // Ki?m tra username ð? t?n t?i chýa
                bool exists = await _dbManager.UserRepository.IsUsernameExistsAsync(Username);

                Debug.WriteLine($"[SignupViewModel] Username '{Username}' exists: {exists}");

                if (exists)
                {
                    RaiseError("Th?t b?i", $"Username '{Username}' ð? t?n t?i trong h? th?ng.\nVui l?ng ch?n username khác.");
                    return;
                }

                Debug.WriteLine($"[SignupViewModel] Calling RegisterUserAsync...");

                // Ðãng k? user m?i
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
                    RaiseError("Th?t b?i", "Không th? ðãng k? tài kho?n.\n\nNguyên nhân có th?:\n- Username ð? t?n t?i\n- L?i database\n- Không ð? quy?n\n\nVui l?ng ki?m tra Output window (View ? Output) ð? xem chi ti?t l?i.");
                }
            }
            catch (Npgsql.PostgresException pgEx)
            {
                Debug.WriteLine($"[SignupViewModel] PostgresException: {pgEx.Message}");

                if (pgEx.SqlState == "23505") // Unique violation
                {
                    RaiseError("L?i", $"Username '{Username}' ð? t?n t?i trong database.");
                }
                else
                {
                    RaiseError("L?i Database", $"PostgreSQL Error:\n{pgEx.Message}\n\nCode: {pgEx.SqlState}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignupViewModel] Exception: {ex.GetType().Name}");
                Debug.WriteLine($"[SignupViewModel] Message: {ex.Message}");
                Debug.WriteLine($"[SignupViewModel] StackTrace: {ex.StackTrace}");

                RaiseError("L?i", $"Ð? x?y ra l?i khi ðãng k?:\n\n{ex.Message}\n\nXem Output window (View ? Output) ð? bi?t chi ti?t.");
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
    /// Event args cho ðãng k? thành công
    /// </summary>
    public class SignupSuccessEventArgs : EventArgs
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    #endregion
}
