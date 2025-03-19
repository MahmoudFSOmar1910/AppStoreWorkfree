using System;
using System.Windows.Input;
using Xamarin.Forms;
using DirectSalesApp.Services;
using System.Threading.Tasks;

namespace DirectSalesApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isError;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        public ICommand LoginCommand { get; }

        public event EventHandler<bool> LoginCompleted;

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new Command(async () => await LoginAsync());
            Title = "Login";
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password are required";
                IsError = true;
                return;
            }

            IsBusy = true;
            IsError = false;

            try
            {
                var success = await _authService.LoginAsync(Username, Password);
                
                if (success)
                {
                    // Clear fields
                    Password = string.Empty;
                    
                    // Notify success
                    LoginCompleted?.Invoke(this, true);
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                    IsError = true;
                    LoginCompleted?.Invoke(this, false);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
                IsError = true;
                LoginCompleted?.Invoke(this, false);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 