using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DirectSalesApp.Views;
using DirectSalesApp.Services;

namespace DirectSalesApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Set the application's main page to start with the splash screen
            MainPage = new NavigationPage(new SplashPage())
            {
                BarBackgroundColor = Color.FromHex("#2196F3"),
                BarTextColor = Color.White
            };
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
} 