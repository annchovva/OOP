using System;
using System.Windows;
using FinancialSystem.Application.Interfaces;

namespace FinancialSystem.UI
{
    public partial class TransferWindow : Window
    {
        private readonly int _fromAccountId;
        private readonly IBankService _bankService;

        public TransferWindow(int fromId, IBankService bankService)
        {
            InitializeComponent();
            _fromAccountId = fromId;
            _bankService = bankService;

            // Установка фокуса на поле номера счета при открытии
            ToAccountTextBox.Focus();
        }

        private void Transfer_Click(object sender, RoutedEventArgs e)
        {
            string recipientNumber = ToAccountTextBox.Text.Trim();
            string amountText = AmountTextBox.Text.Replace(".", ",");

            // 1. Проверка номера счета
            if (string.IsNullOrWhiteSpace(recipientNumber))
            {
                MessageBox.Show("Введите номер счета получателя.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Проверка суммы
            if (!decimal.TryParse(amountText, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму перевода.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 3. Выполнение операции через сервис
                bool success = _bankService.TransferMoney(_fromAccountId, recipientNumber, amount);

                if (success)
                {
                    MessageBox.Show($"Перевод на сумму {amount:N2} ₽ успешно выполнен!",
                                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Ошибка перевода. Возможные причины:\n" +
                                    "— Недостаточно средств на счете\n" +
                                    "— Счет получателя не найден\n" +
                                    "— Один из счетов заблокирован",
                                    "Ошибка операции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла системная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
