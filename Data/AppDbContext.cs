using System;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using DirectSalesApp.Models;
using Xamarin.Essentials;

namespace DirectSalesApp.Data
{
    public class AppDbContext
    {
        readonly SQLiteAsyncConnection _database;

        public AppDbContext()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "DirectSales.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            
            // Create tables
            _database.CreateTableAsync<Product>().Wait();
            _database.CreateTableAsync<User>().Wait();
            _database.CreateTableAsync<Supplier>().Wait();
            _database.CreateTableAsync<Sale>().Wait();
            _database.CreateTableAsync<SaleItem>().Wait();
            _database.CreateTableAsync<Notification>().Wait();
            
            // Initialize database with default data
            InitializeDatabaseAsync().Wait();
        }

        private async Task InitializeDatabaseAsync()
        {
            // Check if admin user exists
            var adminUser = await _database.Table<User>()
                .Where(u => u.Username == "admin")
                .FirstOrDefaultAsync();
                
            // If no admin user exists, create one
            if (adminUser == null)
            {
                var defaultAdmin = new User
                {
                    Username = "admin",
                    Password = "admin123", // In production, this should be hashed
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@directsales.com",
                    PhoneNumber = "1234567890",
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.Now,
                    LastLogin = DateTime.Now,
                    IsActive = true
                };
                
                await SaveUserAsync(defaultAdmin);
                
                // You could also add sample products, suppliers, etc. here
            }
        }

        // Product operations
        public Task<int> SaveProductAsync(Product product)
        {
            if (product.ProductId != 0)
                return _database.UpdateAsync(product);
            else
            {
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;
                return _database.InsertAsync(product);
            }
        }

        public Task<int> DeleteProductAsync(Product product)
        {
            return _database.DeleteAsync(product);
        }

        public Task<Product> GetProductAsync(int id)
        {
            return _database.Table<Product>().Where(p => p.ProductId == id).FirstOrDefaultAsync();
        }

        public Task<Product> GetProductByBarcodeAsync(string barcode)
        {
            return _database.Table<Product>().Where(p => p.Barcode == barcode).FirstOrDefaultAsync();
        }

        public Task<System.Collections.Generic.List<Product>> GetProductsAsync()
        {
            return _database.Table<Product>().ToListAsync();
        }

        public Task<System.Collections.Generic.List<Product>> GetLowStockProductsAsync()
        {
            return _database.Table<Product>().Where(p => p.Quantity <= p.MinimumStockLevel).ToListAsync();
        }

        public Task<System.Collections.Generic.List<Product>> GetExpiringProductsAsync(int daysThreshold = 30)
        {
            var thresholdDate = DateTime.Now.AddDays(daysThreshold);
            return _database.Table<Product>().Where(p => p.ExpirationDate <= thresholdDate).ToListAsync();
        }

        // Similar methods for Users, Suppliers, Sales, and Notifications
        // User operations
        public Task<int> SaveUserAsync(User user)
        {
            if (user.UserId != 0)
                return _database.UpdateAsync(user);
            else
            {
                user.CreatedAt = DateTime.Now;
                return _database.InsertAsync(user);
            }
        }

        public Task<User> GetUserAsync(int id)
        {
            return _database.Table<User>().Where(u => u.UserId == id).FirstOrDefaultAsync();
        }

        public Task<User> AuthenticateUserAsync(string username, string password)
        {
            return _database.Table<User>().Where(u => u.Username == username && u.Password == password).FirstOrDefaultAsync();
        }

        // Sale operations
        public async Task<int> SaveSaleAsync(Sale sale)
        {
            if (sale.SaleId != 0)
                return await _database.UpdateAsync(sale);
            else
            {
                sale.SaleDate = DateTime.Now;
                var id = await _database.InsertAsync(sale);
                
                // Save each sale item
                foreach (var item in sale.Items)
                {
                    item.SaleId = sale.SaleId;
                    await _database.InsertAsync(item);
                    
                    // Update product quantity
                    var product = await GetProductAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Quantity -= item.Quantity;
                        product.UpdatedAt = DateTime.Now;
                        
                        // Check if stock is low
                        if (product.Quantity <= product.MinimumStockLevel && !product.IsLowStock)
                        {
                            product.IsLowStock = true;
                            
                            // Create notification
                            var notification = new Notification
                            {
                                ProductId = product.ProductId,
                                Title = "Low Stock Alert",
                                Message = $"Product '{product.Name}' is running low. Current stock: {product.Quantity}",
                                Type = NotificationType.LowStock,
                                CreatedAt = DateTime.Now,
                                IsRead = false
                            };
                            
                            await SaveNotificationAsync(notification);
                        }
                        
                        await _database.UpdateAsync(product);
                    }
                }
                
                return id;
            }
        }

        // Notification operations
        public Task<int> SaveNotificationAsync(Notification notification)
        {
            if (notification.NotificationId != 0)
                return _database.UpdateAsync(notification);
            else
            {
                notification.CreatedAt = DateTime.Now;
                return _database.InsertAsync(notification);
            }
        }

        public Task<System.Collections.Generic.List<Notification>> GetUnreadNotificationsAsync()
        {
            return _database.Table<Notification>().Where(n => !n.IsRead).OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        // Supplier operations
        public Task<int> SaveSupplierAsync(Supplier supplier)
        {
            if (supplier.SupplierId != 0)
                return _database.UpdateAsync(supplier);
            else
            {
                supplier.CreatedAt = DateTime.Now;
                supplier.UpdatedAt = DateTime.Now;
                return _database.InsertAsync(supplier);
            }
        }

        public Task<System.Collections.Generic.List<Supplier>> GetSuppliersAsync()
        {
            return _database.Table<Supplier>().ToListAsync();
        }
    }
} 