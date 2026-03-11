using System.Collections.Generic;
using System.Reflection.Emit;
using FinancialSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystem.Infrastructure
{
    public class FinanceDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Enterprise> Enterprises { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<TransactionRecord> Transactions { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Настраиваем SQLite и прокси для ленивой загрузки
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlite("Data Source=FinancialSystem.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Уникальный логин для пользователей
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();
        }
    }
}

