using System;
using System.Linq;
using System.Windows;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums; // Добавлено для статусов
using FinancialSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystem.UI
{
    public partial class SalaryWindow : Window
    {
        private readonly User _user;
        private readonly IEnterpriseService _enterpriseService;
        private readonly IBankService _bankService;
        private readonly ILogService _logService; // Вынес в поле
        private readonly FinanceDbContext _db;

        public SalaryWindow(User user)
        {
            InitializeComponent();
            _user = user;
            _db = new FinanceDbContext();

            // Инициализируем сервисы
            _logService = new LogService(_db);
            _enterpriseService = new EnterpriseService(_db, _logService);
            _bankService = new BankService(_db, _logService);

            RefreshUI();
        }

        private void RefreshUI()
        {
            try
            {
                var currentUser = _db.Users
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Id == _user.Id);

                if (currentUser == null) return;

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

                    AccountsComboBox.ItemsSource = _bankService.GetUserAccounts(currentUser.Id);
                    var payments = _enterpriseService.GetMyApprovedPayments(currentUser.Id);
                    ApprovedPaymentsGrid.ItemsSource = payments;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", this);
            }
        }

        private void JoinEnterprise_Click(object sender, RoutedEventArgs e)
        {
            if (EnterpriseComboBox.SelectedItem is Enterprise selected)
            {
                try
                {
                    _enterpriseService.SendJoinRequest(_user.Id, selected.Id);

                    // Ищем созданную заявку, чтобы получить её ID для лога
                    var request = _db.SalaryRequests
                        .OrderByDescending(r => r.Id)
                        .FirstOrDefault(r => r.UserId == _user.Id);

                    if (request != null)
                    {
                        _logService.Log(_user.Id, "JoinRequest",
                            $"Заявка в организацию {selected.Name}",
                            $"{request.Id}");
                    }

                    CustomMessageBox.Show(
                        "Заявка на вступление успешно отправлена менеджеру организации.",
                        "Заявка отправлена", this);
                    this.Close();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(ex.Message, "Ошибка", this);
                }
            }
            else
            {
                CustomMessageBox.Show("Пожалуйста, выберите организацию из списка.", "Внимание", this);
            }
        }

        private void RequestPayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _enterpriseService.SendSalaryPaymentRequest(_user.Id);

                // Ищем созданную заявку на выплату
                var request = _db.SalaryRequests
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefault(r => r.UserId == _user.Id);

                if (request != null)
                {
                    _logService.Log(_user.Id, "SalaryRequest",
                        "Запрос на стандартную выплату (50 000 ₽)",
                        $"{request.Id}");
                }

                CustomMessageBox.Show(
                    "Запрос на стандартную выплату (50 000 ₽) успешно сформирован.",
                    "Запрос выплаты", this);
                RefreshUI();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, "Ошибка запроса", this);
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
                            // ЛОГ: Зачисление зарплаты (для отмены)
                            // ТехДанные: ID_Счета;Сумма;ID_Заявки
                            _logService.Log(_user.Id, "ClaimSalary",
                                $"Зачисление зарплаты {request.Amount} на счет {targetAcc.AccountNumber}",
                                $"{targetAcc.Id};{request.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture)};{request.Id}");

                            CustomMessageBox.Show("Средства успешно зачислены!", "Успех", this);
                            RefreshUI();
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show(ex.Message, "Ошибка зачисления", this);
                    }
                }
                else
                {
                    CustomMessageBox.Show("Пожалуйста, сначала выберите счет.", "Выбор счета", this);
                }
            }
        }

        private void Resign_Click(object sender, RoutedEventArgs e)
        {
            bool result = CustomMessageBox.ShowQuestion(
                "Вы уверены, что хотите уволиться? Все текущие заявки будут аннулированы.",
                "Подтверждение увольнения", this);

            if (result)
            {
                try
                {
                    var currentUser = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == _user.Id);
                    int? oldEntId = currentUser?.EnterpriseId;

                    if (oldEntId != null)
                    {
                        _enterpriseService.ResignFromEnterprise(_user.Id);

                        // ЛОГ: Увольнение
                        _logService.Log(_user.Id, "Resign",
                            "Пользователь уволился по собственному желанию",
                            $"{_user.Id};{oldEntId}");

                        CustomMessageBox.Show("Вы успешно уволились.", "Готово", this);
                        RefreshUI();
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show("Ошибка: " + ex.Message, "Ошибка", this);
                }
            }
        }
    }
}
