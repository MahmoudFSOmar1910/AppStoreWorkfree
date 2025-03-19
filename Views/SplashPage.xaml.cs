using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DirectSalesApp.Data;
using DirectSalesApp.Services;

namespace DirectSalesApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SplashPage : ContentPage
    {
        private readonly AuthService _authService;
        
        public SplashPage()
        {
            InitializeComponent();
            _authService = new AuthService();
            
            // Make sure the AppDbContext is initialized, which creates the default admin user
            var dbContext = new AppDbContext();
            
            // Start the navigation process after a short delay
            Device.StartTimer(TimeSpan.FromSeconds(2), () => {
                NavigateToAppropriatePageAsync();
                return false;
            });
        }
        
        private async void NavigateToAppropriatePageAsync()
        {
            // Check if user is already logged in
            bool isAuthenticated = await _authService.IsAuthenticatedAsync();
            
            if (isAuthenticated)
            {
                // User is logged in, navigate to main page
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                // User is not logged in, navigate to login page
                await Navigation.PushAsync(new LoginPage());
            }
            
            // Remove this page from navigation stack
            Navigation.RemovePage(this);
        }
    }
} 