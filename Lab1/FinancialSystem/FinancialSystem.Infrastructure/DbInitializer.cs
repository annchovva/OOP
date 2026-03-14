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

            // 2. Если в базе уже есть пользователи, выходим (чтобы не дублировать данные)
            if (context.Users.Any()) return;

            // --- ДОБАВЛЯЕМ БАНКИ ---
            var banks = new List<Bank>
            {
                new Bank { Name = "ГосБанк РФ" },
                new Bank { Name = "Альфа-Система" },
                new Bank { Name = "Тинькофф Инвест" },
                new Bank { Name = "ВТБ-Групп" }
            };
            context.Banks.AddRange(banks);

            // --- ДОБАВЛЯЕМ ПРЕДПРИЯТИЯ (РАЗНЫЕ ОТРАСЛИ) ---
            var enterprises = new List<Enterprise>
            {
                new Enterprise { Name = "ООО 'ТехноПроект'" },      // IT
                new Enterprise { Name = "ПАО 'ГазДобыча'" },        // Энергетика
                new Enterprise { Name = "ЗАО 'СтройИнвест'" },      // Строительство
                new Enterprise { Name = "Сеть РитейлМаркет" }        // Торговля
            };
            context.Enterprises.AddRange(enterprises);

            // --- ДОБАВЛЯЕМ СЛУЖЕБНЫХ ПОЛЬЗОВАТЕЛЕЙ ---

            // 1. АДМИНИСТРАТОР (Полный доступ)
            context.Users.Add(new User
            {
                Login = "admin",
                PasswordHash = "admin123",
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                IsApproved = true
            });

            // 2. МЕНЕДЖЕР (Для одобрения заявок)
            context.Users.Add(new User
            {
                Login = "manager",
                PasswordHash = "manager123",
                Role = UserRole.Manager,
                Status = UserStatus.Active,
                IsApproved = true
            });

            // 3. ТЕСТОВЫЙ КЛИЕНТ (Уже активен, для быстрых тестов)
            context.Users.Add(new User
            {
                Login = "user",
                PasswordHash = "user123",
                Role = UserRole.Client,
                Status = UserStatus.Active,
                IsApproved = true
            });

            // 4. КЛИЕНТ В ОЖИДАНИИ (Чтобы менеджеру было кого одобрять)
            context.Users.Add(new User
            {
                Login = "new_client",
                PasswordHash = "12345",
                Role = UserRole.Client,
                Status = UserStatus.Pending,
                IsApproved = false
            });

            // Сохраняем все изменения в БД
            context.SaveChanges();
        }
    }
}
