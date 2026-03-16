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

            ToAccountTextBox.Focus();
        }

        private void Transfer_Click(object sender, RoutedEventArgs e)
        {
            string recipientNumber = ToAccountTextBox.Text.Trim();
            string amountText = AmountTextBox.Text.Replace(".", ",");

            if (string.IsNullOrWhiteSpace(recipientNumber))
            {
                CustomMessageBox.Show("Пожалуйста, введите номер счета получателя.", "Внимание", this);
                ToAccountTextBox.Focus();
                return;
            }

            if (!decimal.TryParse(amountText, out decimal amount) || amount <= 0)
            {
                CustomMessageBox.Show("Введите корректную сумму перевода (положительное число).", "Ошибка ввода", this);
                AmountTextBox.Focus();
                return;
            }

            try
            {
                bool success = _bankService.TransferMoney(_fromAccountId, recipientNumber, amount);

                if (success)
                {
                    CustomMessageBox.Show(
                        $"Перевод на сумму {amount:N2} ₽ успешно выполнен!",
                        "Успех",
                        this);

                    DialogResult = true; 
                }
                else
                {

                    CustomMessageBox.Show(
                        "Не удалось выполнить перевод.",
                        "Ошибка операции",
                        this);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(
                    $"Произошла непредвиденная ошибка: {ex.Message}",
                    "Системная ошибка",
                    this);
            }
        }
    }
}
