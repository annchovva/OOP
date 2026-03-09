using System.Collections.Generic;
using System.Reflection.Emit;
using FinancialSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystem.Infrastructure.Data;

// Наследуемся от DbContext - главного класса Entity Framework
public class FinancialDbContext : DbContext
{
    // DbSet - это представление таблиц в базе данных
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Bank> Banks { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Deposit> Deposits { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Enterprise> Enterprises { get; set; } = null!;

    // Пустой конструктор (понадобится для миграций)
    public FinancialDbContext()
    {
        // Эта команда проверяет, существует ли база и таблицы. 
        // Если их нет - она их автоматически создаст!
        Database.EnsureCreated();
    }

    // Конструктор для передачи настроек (понадобится позже)
    public FinancialDbContext(DbContextOptions<FinancialDbContext> options)
        : base(options)
    {
    }

    // Настраиваем подключение к SQLite
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Указываем, что используем SQLite и задаем имя файла БД
            optionsBuilder.UseSqlite("Data Source=financial_system.db");
        }
    }

    // Здесь можно точечно настроить связи или правила базы данных
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Пример настройки: у транзакции два внешних ключа на Account.
        // Чтобы EF Core не запутался при удалении, отключаем каскадное удаление
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.FromAccount)
            .WithMany()
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.ToAccount)
            .WithMany()
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


