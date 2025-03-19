using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using DirectSalesApp.Models;
using DirectSalesApp.Data;
using System.Threading.Tasks;
using System.Linq;

namespace DirectSalesApp.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly AppDbContext _dbContext;
        
        private ObservableCollection<Product> _products;
        private Product _selectedProduct;
        private string _searchText;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set 
            { 
                SetProperty(ref _selectedProduct, value);
                OnProductSelected(value);
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                if (!string.IsNullOrEmpty(value))
                    SearchProducts(value);
                else
                    LoadProductsAsync();
            }
        }

        public ICommand LoadProductsCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand RefreshCommand { get; }

        public event EventHandler<Product> ProductSelected;
        public event EventHandler AddNewProduct;

        public ProductsViewModel()
        {
            Title = "Products";
            _dbContext = new AppDbContext();
            Products = new ObservableCollection<Product>();
            
            LoadProductsCommand = new Command(async () => await LoadProductsAsync());
            AddProductCommand = new Command(() => OnAddProduct());
            DeleteProductCommand = new Command<Product>(async (product) => await DeleteProductAsync(product));
            RefreshCommand = new Command(async () => await LoadProductsAsync());
            
            // Load data
            LoadProductsAsync();
        }

        async Task LoadProductsAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Products.Clear();
                var products = await _dbContext.GetProductsAsync();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading products: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task DeleteProductAsync(Product product)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                await _dbContext.DeleteProductAsync(product);
                Products.Remove(product);
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error deleting product: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        void SearchProducts(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            IsBusy = true;

            try
            {
                var filteredProducts = Products.Where(p => 
                    p.Name.ToLower().Contains(searchText.ToLower()) || 
                    p.Barcode.ToLower().Contains(searchText.ToLower()) ||
                    p.Description.ToLower().Contains(searchText.ToLower())).ToList();
                
                Products.Clear();
                foreach (var product in filteredProducts)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error searching products: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        void OnProductSelected(Product product)
        {
            if (product == null)
                return;

            ProductSelected?.Invoke(this, product);
        }

        void OnAddProduct()
        {
            AddNewProduct?.Invoke(this, EventArgs.Empty);
        }
    }
} 