using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using System.Linq;
using System.Collections.Generic;

namespace FinancialSystem.Infrastructure
{
    public static class DbInitializer
    {
        public static void Initialize(FinanceDbContext context)
        {
            // 1. Создаем базу, если её нет
            context.Database.EnsureCreated();

            // 2. Если в базе уже есть пользователи, значит данные инициализированы — выходим
            if (context.Users.Any()) return;

            // --- ДОБАВЛЯЕМ БАНКИ ---
            var banks = new List<Bank>
            {
                new Bank { Name = "Беларусбанк" },
                new Bank { Name = "Альфа-Банк" },
                new Bank { Name = "Приорбанк" },
            };
            context.Banks.AddRange(banks);

            // --- ДОБАВЛЯЕМ ПРЕДПРИЯТИЯ (РАЗНЫЕ ОТРАСЛИ) ---
            var enterprises = new List<Enterprise>
            {
                new Enterprise { Name = "ООО 'ТехноПроект'" },       // IT
                new Enterprise { Name = "Сеть 'МаркетРитейл'" },      // Торговля
                new Enterprise { Name = "ООО 'ЭнергоСеть'" },        // Энергетика
                new Enterprise { Name = "СтройМастер Групп" },        // Строительство
            };
            context.Enterprises.AddRange(enterprises);

            // --- ДОБАВЛЯЕМ СЛУЖЕБНЫХ ПОЛЬЗОВАТЕЛЕЙ (С ХЭШИРОВАНИЕМ) ---

            // 1. АДМИНИСТРАТОР
            context.Users.Add(new User
            {
                Login = "admin",
                PasswordHash = PasswordHasher.HashPassword("admin123"),
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                IsApproved = true
            });

            // 2. МЕНЕДЖЕР
            context.Users.Add(new User
            {
                Login = "manager",
                PasswordHash = PasswordHasher.HashPassword("manager123"),
                Role = UserRole.Manager,
                Status = UserStatus.Active,
                IsApproved = true
            });

            // 3. ТЕСТОВЫЙ КЛИЕНТ
            context.Users.Add(new User
            {
                Login = "user",
                PasswordHash = PasswordHasher.HashPassword("user123"),
                Role = UserRole.Client,
                Status = UserStatus.Active,
                IsApproved = true
            });

            // 4. КЛИЕНТ В ОЖИДАНИИ
            context.Users.Add(new User
            {
                Login = "new_client",
                PasswordHash = PasswordHasher.HashPassword("12345"),
                Role = UserRole.Client,
                Status = UserStatus.Pending,
                IsApproved = false
            });

            // Сохраняем всё в БД
            context.SaveChanges();
        }
    }
}
