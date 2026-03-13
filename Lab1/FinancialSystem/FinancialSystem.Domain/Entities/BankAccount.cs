using System;
using FinancialSystem.Domain.Enums;

namespace FinancialSystem.Domain.Entities
{
    public class BankAccount
    {
        public int Id { get; set; }

        // Связь с пользователем
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Связь с банком
        public int BankId { get; set; }
        public virtual Bank Bank { get; set; } = null!;

        // Основные данные счета
        public string AccountNumber { get; set; } = string.Empty;
        public AccountType Type { get; set; } // Current (счет) или Deposit (вклад)
        public decimal Balance { get; set; }

        // --- НОВЫЕ ПОЛЯ ДЛЯ ВКЛАДОВ (НАКОПЛЕНИЯ) ---

        /// <summary>
        /// Процентная ставка (например, 12.5 для 12.5% годовых).
        /// Используется для расчета накоплений.
        /// </summary>
        public double InterestRate { get; set; }

        /// <summary>
        /// Дата последнего начисления процентов. 
        /// Помогает избежать повторного начисления за один и тот же период.
        /// </summary>
        public DateTime? LastInterestAccrual { get; set; }

        // --- СТАТУС И ДАТЫ ---

        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
