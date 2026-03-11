using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;
using FinancialSystem.Application.Interfaces;
using System.Linq;

namespace FinancialSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly FinanceDbContext _db;

        public AuthService(FinanceDbContext context)
        {
            _db = context;
        }

        public User? Login(string login, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == password);

            if (user != null)
            {
                if (user.Status == UserStatus.Pending)
                    throw new Exception("Ваша учетная запись ожидает подтверждения.");
                if (user.Status == UserStatus.Blocked)
                    throw new Exception("Ваш аккаунт заблокирован.");
            }
            return user;
        }

        public bool Register(string login, string password)
        {
            if (_db.Users.Any(u => u.Login == login))
                return false; // Логин занят

            var newUser = new User
            {
                Login = login,
                PasswordHash = password,
                Role = UserRole.Client,
                Status = UserStatus.Pending // Ждет подтверждения менеджера (по условию лабы)
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();
            return true;
        }
    }
}

