using System;

namespace DirectSalesApp.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int? ProductId { get; set; } // Nullable in case notification isn't about a specific product
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; } // When the notification was read
        public bool IsRead { get; set; }
        public int? UserId { get; set; } // Target user if applicable
    }

    public enum NotificationType
    {
        LowStock,
        ExpirationWarning,
        SystemAlert,
        OrderReceived,
        OrderCompleted,
        SaleAlert,
        Other
    }
} 