using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using DirectSalesApp.Models;
using DirectSalesApp.Data;
using DirectSalesApp.Services;
using System.Threading.Tasks;
using System.Linq;

namespace DirectSalesApp.ViewModels
{
    public class SalesViewModel : BaseViewModel
    {
        private readonly AppDbContext _dbContext;
        private readonly BarcodeScannerService _barcodeService;
        private readonly AuthService _authService;
        
        private ObservableCollection<SaleItemViewModel> _items;
        private decimal _subTotal;
        private decimal _tax;
        private decimal _discount;
        private decimal _total;
        private string _barcode;
        private string _customerName;
        private string _customerPhone;
        private string _customerAddress;
        private string _customerCity;
        private string _customerState;
        private string _customerZipCode;
        private PaymentMethod _selectedPaymentMethod;
        private string _paymentReference;
        private string _errorMessage;
        private bool _isError;
        private string _invoiceNumber;
        private string _cashierName;

        public ObservableCollection<SaleItemViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public decimal SubTotal
        {
            get => _subTotal;
            set => SetProperty(ref _subTotal, value);
        }

        public decimal Tax
        {
            get => _tax;
            set => SetProperty(ref _tax, value);
        }

        public decimal Discount
        {
            get => _discount;
            set => SetProperty(ref _discount, value);
        }

        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public string Barcode
        {
            get => _barcode;
            set
            {
                SetProperty(ref _barcode, value);
                if (!string.IsNullOrEmpty(value))
                    AddProductByBarcodeAsync(value);
            }
        }

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public string CustomerPhone
        {
            get => _customerPhone;
            set => SetProperty(ref _customerPhone, value);
        }

        public string CustomerAddress
        {
            get => _customerAddress;
            set => SetProperty(ref _customerAddress, value);
        }

        public string CustomerCity
        {
            get => _customerCity;
            set => SetProperty(ref _customerCity, value);
        }

        public string CustomerState
        {
            get => _customerState;
            set => SetProperty(ref _customerState, value);
        }

        public string CustomerZipCode
        {
            get => _customerZipCode;
            set => SetProperty(ref _customerZipCode, value);
        }

        public PaymentMethod SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set => SetProperty(ref _selectedPaymentMethod, value);
        }

        public string PaymentReference
        {
            get => _paymentReference;
            set => SetProperty(ref _paymentReference, value);
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

        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set => SetProperty(ref _invoiceNumber, value);
        }

        public string CashierName
        {
            get => _cashierName;
            set => SetProperty(ref _cashierName, value);
        }

        public ICommand ScanBarcodeCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearSaleCommand { get; }
        public ICommand CompleteSaleCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand GenerateInvoiceNumberCommand { get; }

        public event EventHandler<bool> SaleCompleted;

        public SalesViewModel()
        {
            Title = "New Sale";
            _dbContext = new AppDbContext();
            _barcodeService = new BarcodeScannerService();
            _authService = new AuthService();
            
            Items = new ObservableCollection<SaleItemViewModel>();
            
            ScanBarcodeCommand = new Command(async () => await ScanBarcodeAsync());
            AddItemCommand = new Command<string>(async (barcode) => await AddProductByBarcodeAsync(barcode));
            RemoveItemCommand = new Command<SaleItemViewModel>(RemoveItem);
            ClearSaleCommand = new Command(ClearSale);
            CompleteSaleCommand = new Command(async () => await CompleteSaleAsync());
            IncreaseQuantityCommand = new Command<SaleItemViewModel>(IncreaseQuantity);
            DecreaseQuantityCommand = new Command<SaleItemViewModel>(DecreaseQuantity);
            GenerateInvoiceNumberCommand = new Command(GenerateInvoiceNumber);
            
            // Default values
            SelectedPaymentMethod = PaymentMethod.Cash;
            
            // Generate invoice number and load cashier info
            GenerateInvoiceNumber();
            LoadCashierInfoAsync();
        }

        private void GenerateInvoiceNumber()
        {
            // Format: INV-YYYYMMDD-XXXX where XXXX is a random number
            string date = DateTime.Now.ToString("yyyyMMdd");
            string random = new Random().Next(1000, 9999).ToString();
            InvoiceNumber = $"INV-{date}-{random}";
        }

        private async Task LoadCashierInfoAsync()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                CashierName = $"{currentUser.FirstName} {currentUser.LastName}";
            }
        }

        async Task ScanBarcodeAsync()
        {
            try
            {
                var barcode = await _barcodeService.ScanAsync();
                if (!string.IsNullOrEmpty(barcode))
                {
                    Barcode = barcode;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Scanning error: {ex.Message}";
                IsError = true;
            }
        }

        async Task AddProductByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
                return;

            IsBusy = true;
            IsError = false;

            try
            {
                var product = await _dbContext.GetProductByBarcodeAsync(barcode);
                
                if (product == null)
                {
                    ErrorMessage = $"Product with barcode {barcode} not found";
                    IsError = true;
                    return;
                }
                
                // Check if product already in cart
                var existingItem = Items.FirstOrDefault(i => i.ProductId == product.ProductId);
                if (existingItem != null)
                {
                    // Increase quantity
                    existingItem.Quantity++;
                    existingItem.CalculateTotal();
                }
                else
                {
                    // Add new item
                    var saleItem = new SaleItemViewModel
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        UnitPrice = product.Price,
                        Quantity = 1,
                        Discount = 0,
                        Total = product.Price
                    };
                    
                    Items.Add(saleItem);
                }
                
                // Clear barcode field
                Barcode = string.Empty;
                
                // Recalculate totals
                CalculateTotals();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding product: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        void RemoveItem(SaleItemViewModel item)
        {
            if (item == null)
                return;
                
            Items.Remove(item);
            CalculateTotals();
        }

        void IncreaseQuantity(SaleItemViewModel item)
        {
            if (item == null)
                return;
                
            item.Quantity++;
            item.CalculateTotal();
            CalculateTotals();
        }

        void DecreaseQuantity(SaleItemViewModel item)
        {
            if (item == null || item.Quantity <= 1)
                return;
                
            item.Quantity--;
            item.CalculateTotal();
            CalculateTotals();
        }

        void ClearSale()
        {
            Items.Clear();
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CustomerAddress = string.Empty;
            CustomerCity = string.Empty;
            CustomerState = string.Empty;
            CustomerZipCode = string.Empty;
            PaymentReference = string.Empty;
            SelectedPaymentMethod = PaymentMethod.Cash;
            CalculateTotals();
            GenerateInvoiceNumber();
        }

        async Task CompleteSaleAsync()
        {
            if (!Items.Any())
            {
                ErrorMessage = "Cannot complete sale with no items";
                IsError = true;
                return;
            }

            IsBusy = true;
            IsError = false;

            try
            {
                // Get current user
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "User not authenticated";
                    IsError = true;
                    return;
                }
                
                // Create sale object
                var sale = new Sale
                {
                    SaleId = 0, // Will be set by the database
                    UserId = user.UserId,
                    SaleDate = DateTime.Now,
                    SubTotal = SubTotal,
                    TaxAmount = Tax,
                    DiscountAmount = Discount,
                    TotalAmount = Total,
                    PaymentMethod = SelectedPaymentMethod,
                    PaymentReference = PaymentReference,
                    CustomerName = CustomerName,
                    CustomerPhone = CustomerPhone,
                    CustomerAddress = CustomerAddress,
                    CustomerCity = CustomerCity,
                    CustomerState = CustomerState,
                    CustomerZipCode = CustomerZipCode,
                    InvoiceNumber = InvoiceNumber,
                    Notes = $"Sale processed by {CashierName} on {DateTime.Now}",
                    Items = Items.Select(i => new SaleItem
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        Discount = i.Discount,
                        Total = i.Total
                    }).ToList()
                };
                
                // Save to database
                await _dbContext.SaveSaleAsync(sale);
                
                // Clear the sale
                ClearSale();
                
                // Notify completion
                SaleCompleted?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error completing sale: {ex.Message}";
                IsError = true;
                SaleCompleted?.Invoke(this, false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        void CalculateTotals()
        {
            SubTotal = Items.Sum(i => i.Total);
            
            // Default tax rate of 10% - in a real app, this would be configurable
            Tax = Math.Round(SubTotal * 0.1m, 2);
            
            Total = SubTotal + Tax - Discount;
        }
    }

    public class SaleItemViewModel : BaseViewModel
    {
        private int _productId;
        private string _productName;
        private decimal _unitPrice;
        private int _quantity;
        private decimal _discount;
        private decimal _total;

        public int ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        public string ProductName
        {
            get => _productName;
            set => SetProperty(ref _productName, value);
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set => SetProperty(ref _unitPrice, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public decimal Discount
        {
            get => _discount;
            set => SetProperty(ref _discount, value);
        }

        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public void CalculateTotal()
        {
            Total = (UnitPrice * Quantity) - Discount;
        }
    }
} 