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

            // Уникальный логин (оставляем как есть)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            // НАСТРОЙКА ТРАНЗАКЦИЙ (ОБНОВЛЕННАЯ)
            modelBuilder.Entity<TransactionRecord>(entity =>
            {
                // Связь для отправителя
                entity.HasOne(t => t.FromAccount) // Указываем на свойство-объект
                      .WithMany()                 // У одного счета может быть много исходящих транзакций
                      .HasForeignKey(t => t.FromAccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Связь для получателя
                entity.HasOne(t => t.ToAccount)   // Указываем на свойство-объект
                      .WithMany()                 // У одного счета может быть много входящих транзакций
                      .HasForeignKey(t => t.ToAccountId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
