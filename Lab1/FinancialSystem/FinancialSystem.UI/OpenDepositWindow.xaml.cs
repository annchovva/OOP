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
                CustomMessageBox.Show("Ошибка загрузки списка банков: " + ex.Message, "Ошибка", this);
            }
        }

        private void OpenDeposit_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация выбора банка
            if (BankComboBox.SelectedItem is not Bank selectedBank)
            {
                CustomMessageBox.Show("Пожалуйста, выберите банк из списка.", "Внимание", this);
                return;
            }

            // 2. Валидация суммы (заменяем точку на запятую для корректного парсинга)
            string amountText = AmountBox.Text.Replace(".", ",");
            if (!decimal.TryParse(amountText, out decimal amount) || amount <= 0)
            {
                CustomMessageBox.Show("Введите корректную сумму вклада (положительное число).", "Ошибка ввода", this);
                AmountBox.Focus();
                return;
            }

            try
            {
                // 3. Вызов сервиса открытия вклада
                _bankService.OpenDeposit(_userId, selectedBank.Id, amount, (decimal)RateSlider.Value);

                CustomMessageBox.Show(
                    $"Поздравляем!\nВклад в банке «{selectedBank.Name}» успешно открыт.\n" +
                    $"Сумма: {amount:N2} ₽\nСтавка: {RateSlider.Value}%",
                    "Успех",
                    this);

                this.DialogResult = true; // Закрываем окно с успехом
            }
            catch (Exception ex)
            {
                // ИСПРАВЛЕНО: Заменен стандартный MessageBox на CustomMessageBox
                CustomMessageBox.Show("Не удалось открыть вклад: " + ex.Message, "Ошибка", this);
            }
        }
    }
}
