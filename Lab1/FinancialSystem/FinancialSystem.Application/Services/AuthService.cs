using System;
using System.Linq;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure; // тут хэшер
using Microsoft.EntityFrameworkCore;

namespace FinancialSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly FinanceDbContext _db;

        public AuthService(FinanceDbContext db)
        {
            _db = db;
        }

        public User Login(string login, string password)
        {
            string enteredHash = PasswordHasher.HashPassword(password);
            var user = _db.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == enteredHash);

            if (user == null) return null;

            // клиент не одобрен менеджером
            if (!user.IsApproved)
            {
                throw new InvalidOperationException("NOT_APPROVED");
            }

            return user;
        }

        public bool Register(string login, string password)
        {
            // проверка на существование логина
            if (_db.Users.Any(u => u.Login == login))
                return false; 

            string hashedPassword = PasswordHasher.HashPassword(password);

            var newUser = new User
            {
                Login = login,
                PasswordHash = hashedPassword,
                Role = UserRole.Client,
                IsApproved = false
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();
            return true;
        }
    }
}

