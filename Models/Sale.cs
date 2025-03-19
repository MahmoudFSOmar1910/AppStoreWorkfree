using System;
using System.Collections.Generic;

namespace DirectSalesApp.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public int UserId { get; set; } // Cashier who processed the sale
        public DateTime SaleDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentReference { get; set; } // For card or other payment references
        public string CustomerName { get; set; } // Optional customer info
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerZipCode { get; set; }
        public string InvoiceNumber { get; set; }
        public string Notes { get; set; }
        public List<SaleItem> Items { get; set; } = new List<SaleItem>();
    }

    public class SaleItem
    {
        public int SaleItemId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } // Store the name at time of sale
        public decimal UnitPrice { get; set; } // Store the price at time of sale
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
    }

    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        DebitCard,
        BankTransfer,
        MobileMoney,
        Other
    }
} 