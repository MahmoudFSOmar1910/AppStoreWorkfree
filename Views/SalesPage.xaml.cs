using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DirectSalesApp.ViewModels;

namespace DirectSalesApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SalesPage : ContentPage
    {
        private readonly SalesViewModel _viewModel;

        public SalesPage()
        {
            InitializeComponent();
            _viewModel = BindingContext as SalesViewModel;
            _viewModel.SaleCompleted += OnSaleCompleted;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.SaleCompleted -= OnSaleCompleted;
        }

        private async void OnSaleCompleted(object sender, bool success)
        {
            if (success)
            {
                await DisplayAlert("Success", "Sale completed successfully", "OK");
            }
        }
    }
} 