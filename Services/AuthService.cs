using System;
using System.Threading.Tasks;
using DirectSalesApp.Models;
using DirectSalesApp.Data;
using Xamarin.Essentials;
using System.Text;
using System.Security.Cryptography;

namespace DirectSalesApp.Services
{
    public class AuthService
    {
        private readonly AppDbContext _dbContext;
        private const string AUTH_KEY = "auth_user_id";
        private User _currentUser;

        public AuthService()
        {
            _dbContext = new AppDbContext();
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            // NOTE: In the demo with the default admin user, we're still using plain text password
            // For new users, we would hash the password before comparing
            
            var user = await _dbContext.AuthenticateUserAsync(username, password);
            
            if (user != null)
            {
                // Save login info
                _currentUser = user;
                user.LastLogin = DateTime.Now;
                await _dbContext.SaveUserAsync(user);
                
                // Save user ID to preferences
                Preferences.Set(AUTH_KEY, user.UserId);
                
                return true;
            }
            
            return false;
        }

        public async Task<bool> RegisterUserAsync(User newUser, string password)
        {
            // Check if username already exists
            var existingUser = await _dbContext.Table<User>().Where(u => u.Username == newUser.Username).FirstOrDefaultAsync();
            if (existingUser != null)
                return false;
                
            // In a real app, hash the password before saving
            newUser.Password = password; // For demo purposes, we're using plain text
            // In production: newUser.Password = HashPassword(password);
            
            await _dbContext.SaveUserAsync(newUser);
            return true;
        }

        public void Logout()
        {
            _currentUser = null;
            Preferences.Remove(AUTH_KEY);
        }

        public async Task<User> GetCurrentUserAsync()
        {
            if (_currentUser != null)
                return _currentUser;
                
            var userId = Preferences.Get(AUTH_KEY, 0);
            if (userId > 0)
            {
                _currentUser = await _dbContext.GetUserAsync(userId);
                return _currentUser;
            }
            
            return null;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var user = await GetCurrentUserAsync();
            return user != null;
        }

        public async Task<bool> HasPermissionAsync(UserPermission permission)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return false;
                
            switch (permission)
            {
                case UserPermission.ManageProducts:
                    return user.Role == UserRole.Admin || user.Role == UserRole.Manager || user.Role == UserRole.Inventory;
                    
                case UserPermission.ManageUsers:
                    return user.Role == UserRole.Admin;
                    
                case UserPermission.ManageSuppliers:
                    return user.Role == UserRole.Admin || user.Role == UserRole.Manager;
                    
                case UserPermission.ProcessSales:
                    return user.Role == UserRole.Admin || user.Role == UserRole.Manager || user.Role == UserRole.Cashier;
                    
                case UserPermission.ViewReports:
                    return user.Role == UserRole.Admin || user.Role == UserRole.Manager;
                    
                default:
                    return false;
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    public enum UserPermission
    {
        ManageProducts,
        ManageUsers,
        ManageSuppliers,
        ProcessSales,
        ViewReports
    }
} 