using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DirectSalesApp.Models;
using DirectSalesApp.Data;
using Xamarin.Forms;
using Plugin.LocalNotifications;

namespace DirectSalesApp.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _dbContext;

        public NotificationService()
        {
            _dbContext = new AppDbContext();
        }

        public async Task CheckLowStockAsync()
        {
            var lowStockProducts = await _dbContext.GetLowStockProductsAsync();
            
            foreach (var product in lowStockProducts)
            {
                // Create notification in database
                var notification = new Notification
                {
                    ProductId = product.ProductId,
                    Title = "Low Stock Alert",
                    Message = $"Product '{product.Name}' is running low. Current stock: {product.Quantity}",
                    Type = NotificationType.LowStock,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                
                await _dbContext.SaveNotificationAsync(notification);
                
                // Send local notification
                CrossLocalNotifications.Current.Show(
                    "Low Stock Alert",
                    $"Product '{product.Name}' is running low. Current stock: {product.Quantity}",
                    notification.NotificationId);
            }
        }

        public async Task CheckExpiringProductsAsync(int daysThreshold = 30)
        {
            var expiringProducts = await _dbContext.GetExpiringProductsAsync(daysThreshold);
            
            foreach (var product in expiringProducts)
            {
                var daysRemaining = (product.ExpirationDate - DateTime.Now).Days;
                
                // Create notification in database
                var notification = new Notification
                {
                    ProductId = product.ProductId,
                    Title = "Expiration Warning",
                    Message = $"Product '{product.Name}' will expire in {daysRemaining} days (on {product.ExpirationDate.ToShortDateString()})",
                    Type = NotificationType.ExpirationWarning,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                
                await _dbContext.SaveNotificationAsync(notification);
                
                // Send local notification
                CrossLocalNotifications.Current.Show(
                    "Expiration Warning",
                    $"Product '{product.Name}' will expire in {daysRemaining} days",
                    notification.NotificationId + 1000); // Using different ID range
            }
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync()
        {
            return await _dbContext.GetUnreadNotificationsAsync();
        }

        public async Task MarkNotificationAsReadAsync(Notification notification)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;
            await _dbContext.SaveNotificationAsync(notification);
        }
    }
} 