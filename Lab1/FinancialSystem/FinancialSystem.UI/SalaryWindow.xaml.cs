using System;
using System.Linq;
using System.Windows;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Infrastructure;

namespace FinancialSystem.UI
{
    public partial class SalaryWindow : Window
    {
        private readonly User _user;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IBankService _bankService;
        private readonly FinanceDbContext _db;

        public SalaryWindow(User user)
        {
            InitializeComponent();
            _user = user;
            _db = new FinanceDbContext();

            // Инициализируем сервисы с общим контекстом
            var logService = new LogService(_db);
            _enterpriseService = new EnterpriseService(_db, logService);
            _bankService = new BankService(_db);

            RefreshUI();
        }

        private void RefreshUI()
        {
            try
            {
                // Перезагружаем пользователя из БД, чтобы иметь актуальный EnterpriseId
                var currentUser = _db.Users.Find(_user.Id);

                if (currentUser.EnterpriseId == null)
                {
                    NotEmployedPanel.Visibility = Visibility.Visible;
                    EmployedPanel.Visibility = Visibility.Collapsed;
                    EnterpriseComboBox.ItemsSource = _enterpriseService.GetAllEnterprises();
                }
                else
                {
                    NotEmployedPanel.Visibility = Visibility.Collapsed;
                    EmployedPanel.Visibility = Visibility.Visible;

                    var myEnt = _db.Enterprises.Find(currentUser.EnterpriseId);
                    CurrentEnterpriseText.Text = $"Ваше предприятие: {myEnt?.Name}";

                    // Загружаем данные для списков
                    AccountsComboBox.ItemsSource = _bankService.GetUserAccounts(currentUser.Id);
                    var payments = _enterpriseService.GetMyApprovedPayments(currentUser.Id);
                    ApprovedPaymentsGrid.ItemsSource = payments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message);
            }
        }

        private void JoinEnterprise_Click(object sender, RoutedEventArgs e)
        {
            if (EnterpriseComboBox.SelectedItem is Enterprise selected)
            {
                _enterpriseService.SendJoinRequest(_user.Id, selected.Id);
                MessageBox.Show("Заявка на вступление отправлена менеджеру. Ожидайте подтверждения.");
                this.Close();
            }
        }

        private void RequestPayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _enterpriseService.SendSalaryPaymentRequest(_user.Id);
                MessageBox.Show("Запрос на выплату (50 000 ₽) отправлен менеджеру.");
                RefreshUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClaimSalary_Click(object sender, RoutedEventArgs e)
        {
            if (ApprovedPaymentsGrid.SelectedItem is SalaryRequest request)
            {
                if (AccountsComboBox.SelectedItem is BankAccount targetAcc)
                {
                    try
                    {
                        if (_enterpriseService.ClaimSalary(request.Id, targetAcc.Id))
                        {
                            MessageBox.Show("Деньги успешно зачислены на ваш счет!");
                            RefreshUI();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, сначала выберите счет для зачисления внизу окна.");
                }
            }
        }
    }
}
