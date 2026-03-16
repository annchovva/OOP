using System;
using System.Linq;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure; // Не забудьте подключить неймспейс, где лежит Hasher
using Microsoft.EntityFrameworkCore;

namespace FinancialSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly FinanceDbContext _db;

        public AuthService(FinanceDbContext context)
        {
            _db = context;
        }

        public User Login(string login, string password)
        {
            // Хэшируем то, что ввел пользователь прямо сейчас
            string enteredHash = PasswordHasher.HashPassword(password);

            // Ищем в базе пользователя, у которого совпадает логин И хэш
            return _db.Users.FirstOrDefault(u =>
                u.Login == login &&
                u.PasswordHash == enteredHash &&
                u.IsApproved);
        }


        public bool Register(string login, string password)
        {
            if (_db.Users.Any(u => u.Login == login))
                return false; // Логин занят

            // 1. Хэшируем пароль ПЕРЕД сохранением в базу
            string hashedPassword = PasswordHasher.HashPassword(password);

            var newUser = new User
            {
                Login = login,
                PasswordHash = hashedPassword, // Сохраняем уже зашифрованный вид
                Role = UserRole.Client,
                Status = UserStatus.Pending
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();
            return true;
        }
    }
}
