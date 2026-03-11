using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Domain.Interfaces;
using FinancialSystem.Infrastructure;
using System.Linq;

namespace FinancialSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly FinanceDbContext _context;

        public AuthService(FinanceDbContext context)
        {
            _context = context;
        }

        public User? Login(string login, string password)
        {
            // Ищем пользователя по логину и паролю
            // ВАЖНО: В реальных проектах пароли хэшируются, но для лабы сделаем простое сравнение
            var user = _context.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == password);

            if (user == null) return null;

            // Если это клиент, проверяем, подтвердил ли его менеджер
            if (user.Role == UserRole.Client && user.Status == UserStatus.Pending)
            {
                throw new Exception("Ваша регистрация еще не подтверждена менеджером.");
            }

            return user;
        }

        public bool Register(string login, string password)
        {
            if (_context.Users.Any(u => u.Login == login))
                return false; // Логин занят

            var newUser = new User
            {
                Login = login,
                PasswordHash = password,
                Role = UserRole.Client,
                Status = UserStatus.Pending // Ждет подтверждения менеджера (по условию лабы)
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();
            return true;
        }
    }
}

