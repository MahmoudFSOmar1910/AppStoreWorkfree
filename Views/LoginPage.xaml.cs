using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DirectSalesApp.ViewModels;

namespace DirectSalesApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as LoginViewModel;
            _viewModel.LoginCompleted += OnLoginCompleted;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.LoginCompleted -= OnLoginCompleted;
        }

        private async void OnLoginCompleted(object sender, bool success)
        {
            if (success)
            {
                // Navigate to main page
                await Navigation.PushAsync(new MainPage());
            }
        }
    }
} 