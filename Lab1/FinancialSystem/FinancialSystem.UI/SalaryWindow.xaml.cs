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

        public SalaryWindow(User user)
        {
            InitializeComponent();
            _user = user;
            var db = new FinanceDbContext();

            // Инициализируем сервисы
            var logService = new LogService(db);
            _enterpriseService = new EnterpriseService(db, logService);
            _bankService = new BankService(db);

            RefreshUI();
        }

        private void RefreshUI()
        {
            // Проверяем, есть ли у юзера предприятие
            if (_user.EnterpriseId == null)
            {
                NotEmployedPanel.Visibility = Visibility.Visible;
                EmployedPanel.Visibility = Visibility.Collapsed;
                EnterpriseComboBox.ItemsSource = _enterpriseService.GetAllEnterprises();
            }
            else
            {
                NotEmployedPanel.Visibility = Visibility.Collapsed;
                EmployedPanel.Visibility = Visibility.Visible;

                var enterprises = _enterpriseService.GetAllEnterprises();
                var myEnt = enterprises.FirstOrDefault(e => e.Id == _user.EnterpriseId);
                CurrentEnterpriseText.Text = $"Ваше предприятие: {myEnt?.Name}";

                // Загружаем счета для выбора
                AccountsComboBox.ItemsSource = _bankService.GetUserAccounts(_user.Id);
                // Загружаем одобренные выплаты
                ApprovedPaymentsGrid.ItemsSource = _enterpriseService.GetMyApprovedPayments(_user.Id);
            }
        }

        private void JoinEnterprise_Click(object sender, RoutedEventArgs e)
        {
            var selected = EnterpriseComboBox.SelectedItem as Enterprise;
            if (selected == null) return;

            _enterpriseService.SendJoinRequest(_user.Id, selected.Id);
            MessageBox.Show("Заявка отправлена менеджеру!");
            this.Close();
        }

        private void RequestPayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _enterpriseService.SendSalaryPaymentRequest(_user.Id);
                MessageBox.Show("Запрос на выплату отправлен. Ожидайте одобрения менеджером.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClaimSalary_Click(object sender, RoutedEventArgs e)
        {
            var request = (sender as System.Windows.Controls.Button).DataContext as SalaryRequest;
            var targetAcc = AccountsComboBox.SelectedItem as BankAccount;

            if (request == null || targetAcc == null)
            {
                MessageBox.Show("Сначала выберите счет в списке внизу!");
                return;
            }

            try
            {
                if (_enterpriseService.ClaimSalary(request.Id, targetAcc.Id))
                {
                    MessageBox.Show("Деньги успешно зачислены на ваш счет!");
                    RefreshUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
