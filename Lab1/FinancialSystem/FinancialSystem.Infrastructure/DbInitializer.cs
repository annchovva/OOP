using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using System.Linq;

namespace FinancialSystem.Infrastructure
{
    public static class DbInitializer
    {
        public static void Initialize(FinanceDbContext context)
        {
            // Создаем базу, если она еще не создана
            context.Database.EnsureCreated();

            // Если в базе уже есть пользователи, значит инициализация не нужна
            if (context.Users.Any()) return;

            // Добавляем тестовый банк
            var testBank = new Bank { Name = "Центральный Банк" };
            context.Banks.Add(testBank);

            // Добавляем тестовое предприятие
            var testEnterprise = new Enterprise { Name = "IT-Технологии" };
            context.Enterprises.Add(testEnterprise);

            // Добавляем Администратора
            context.Users.Add(new User
            {
                Login = "admin",
                PasswordHash = "admin123", // В реале тут должен быть хэш!
                Role = UserRole.Admin,
                Status = UserStatus.Active
            });

            // Добавляем Менеджера
            context.Users.Add(new User
            {
                Login = "manager",
                PasswordHash = "manager123",
                Role = UserRole.Manager,
                Status = UserStatus.Active
            });

            context.SaveChanges();
        }
    }
}

