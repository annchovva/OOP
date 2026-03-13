using System;
using System.Windows;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.UI
{
    public partial class OpenDepositWindow : Window
    {
        private readonly IBankService _bankService;
        private readonly int _userId;

        public OpenDepositWindow(int userId, IBankService bankService)
        {
            InitializeComponent(); // Теперь эта команда будет работать без ошибок
            _userId = userId;
            _bankService = bankService;

            // Заполняем список банков из базы
            try
            {
                BankComboBox.ItemsSource = _bankService.GetAllBanks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки банков: " + ex.Message);
            }
        }

        private void OpenDeposit_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверка выбора банка
            var bank = BankComboBox.SelectedItem as Bank;
            if (bank == null)
            {
                MessageBox.Show("Пожалуйста, выберите банк.");
                return;
            }

            // 2. Проверка введенной суммы
            if (!decimal.TryParse(AmountBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму вклада (больше 0).");
                return;
            }

            try
            {
                // 3. Вызываем метод создания вклада (мы добавили его в BankService ранее)
                _bankService.OpenDeposit(_userId, bank.Id, amount, RateSlider.Value);

                MessageBox.Show($"Вклад успешно открыт!\nСумма: {amount:N2} ₽\nСтавка: {RateSlider.Value}%");

                this.DialogResult = true; // Закрываем окно с успехом
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при открытии вклада: " + ex.Message);
            }
        }
    }
}
