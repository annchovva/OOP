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

        // Добавляем таблицу для заявок на зарплатные проекты
        public DbSet<SalaryRequest> SalaryRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlite("Data Source=FinancialSystem.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Уникальный логин
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            // Настройка связи "Многие-ко-многим" для сотрудников предприятия (если нужно)
            // Но обычно в простых лабах достаточно связи Один-ко-многим или через SalaryRequest

            // Настройка транзакций (связь с аккаунтами может быть цикличной, 
            // поэтому отключаем каскадное удаление для безопасности)
            modelBuilder.Entity<TransactionRecord>()
                .HasOne<BankAccount>()
                .WithMany()
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionRecord>()
                .HasOne<BankAccount>()
                .WithMany()
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
