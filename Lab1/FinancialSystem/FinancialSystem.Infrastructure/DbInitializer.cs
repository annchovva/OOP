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
            // создаем базу данных
            context.Database.EnsureCreated();

            if (context.Users.Any()) return;

            // банки
            var banks = new List<Bank>
            {
                new Bank { Name = "Беларусбанк" },
                new Bank { Name = "Альфа-Банк" },
                new Bank { Name = "Приорбанк" },
            };
            context.Banks.AddRange(banks);

            // предприятия
            var enterprises = new List<Enterprise>
            {
                new Enterprise { Name = "ООО 'ТехноПроект'" },   
                new Enterprise { Name = "Сеть 'МаркетРитейл'" },   
                new Enterprise { Name = "ООО 'ЭнергоСеть'" },      
                new Enterprise { Name = "СтройМастер Групп" },   
            };
            context.Enterprises.AddRange(enterprises);

            // администратор
            context.Users.Add(new User
            {
                Login = "admin",
                PasswordHash = PasswordHasher.HashPassword("admin123"),
                Role = UserRole.Admin,
                IsApproved = true
            });

            // менеджер
            context.Users.Add(new User
            {
                Login = "manager",
                PasswordHash = PasswordHasher.HashPassword("manager123"),
                Role = UserRole.Manager,
                IsApproved = true
            });

            // одобренный клиент
            context.Users.Add(new User
            {
                Login = "user",
                PasswordHash = PasswordHasher.HashPassword("user123"),
                Role = UserRole.Client,
                IsApproved = true
            });

            // неодобренный клиент
            context.Users.Add(new User
            {
                Login = "new_client",
                PasswordHash = PasswordHasher.HashPassword("12345"),
                Role = UserRole.Client,
                IsApproved = false
            });

            context.SaveChanges();
        }
    }
}

