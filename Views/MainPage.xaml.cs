using System;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DirectSalesApp.Services;

namespace DirectSalesApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : TabbedPage
    {
        private readonly AuthService _authService;
        public ICommand LogoutCommand { get; private set; }
        
        public MainPage()
        {
            InitializeComponent();
            
            _authService = new AuthService();
            LogoutCommand = new Command(OnLogout);
            
            // Set the binding context to this instance (for the LogoutCommand)
            BindingContext = this;
        }
        
        private async void OnLogout()
        {
            bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Yes", "No");
            
            if (confirm)
            {
                // Perform logout
                _authService.Logout();
                
                // Navigate back to login page
                await Navigation.PushAsync(new LoginPage());
                
                // Remove all pages from the navigation stack except the login page
                var existingPages = Navigation.NavigationStack;
                for (int i = 0; i < existingPages.Count - 1; i++)
                {
                    Navigation.RemovePage(existingPages[i]);
                }
            }
        }
    }
} 