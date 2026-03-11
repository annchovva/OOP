using System.Windows;
using System.Windows.Controls;
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Application.Interfaces;

namespace FinancialSystem.UI
{
    public partial class AdminLogsWindow : Window
    {
        private readonly ILogService _logService;

        public AdminLogsWindow()
        {
            InitializeComponent();
            _logService = new LogService(new Infrastructure.FinanceDbContext());
            LoadLogs();
        }

        private void LoadLogs()
        {
            LogsGrid.ItemsSource = _logService.GetAllLogs();
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            var log = (sender as Button).DataContext as ActionLog;
            if (log != null)
            {
                var result = _logService.UndoAction(log.Id);
                if (result)
                {
                    MessageBox.Show("Действие успешно отменено! Балансы счетов скорректированы.");
                    LoadLogs();
                }
                else
                {
                    MessageBox.Show("Не удалось отменить действие (возможно, оно уже отменено).");
                }
            }
        }
    }
}
