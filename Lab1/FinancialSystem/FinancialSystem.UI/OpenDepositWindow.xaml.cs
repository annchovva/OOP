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
            InitializeComponent();
            _userId = userId;
            _bankService = bankService;

            LoadBanks();
        }

        private void LoadBanks()
        {
            try
            {
                var banks = _bankService.GetAllBanks();
                BankComboBox.ItemsSource = banks;
                if (banks != null && banks.Count > 0)
                    BankComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списка банков: " + ex.Message);
            }
        }

        private void OpenDeposit_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация выбора банка
            if (BankComboBox.SelectedItem is not Bank selectedBank)
            {
                MessageBox.Show("Пожалуйста, выберите банк из списка.");
                return;
            }

            // 2. Валидация суммы
            if (!decimal.TryParse(AmountBox.Text.Replace(".", ","), out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму вклада (положительное число).");
                AmountBox.Focus();
                return;
            }

            try
            {
                // 3. Вызов сервиса
                _bankService.OpenDeposit(_userId, selectedBank.Id, amount, (double)RateSlider.Value);

                MessageBox.Show($"Поздравляем!\nВклад в банке «{selectedBank.Name}» успешно открыт.\n" +
                                $"Сумма: {amount:N2} ₽\nСтавка: {RateSlider.Value}%",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть вклад: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
