using System;
using System.Windows;
using System.Windows.Controls;
using FinancialSystem.Application.Services;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Application.Interfaces;
using FinancialSystem.Infrastructure; // Не забываем

namespace FinancialSystem.UI
{
    public partial class AdminLogsWindow : Window
    {
        private readonly ILogService _logService;
        private readonly FinanceDbContext _db;

        public AdminLogsWindow()
        {
            InitializeComponent();

            // Инициализируем базу
            _db = new FinanceDbContext();
            _logService = new LogService(_db);

            LoadLogs();
        }

        private void LoadLogs()
        {
            try
            {
                // Очищаем кэш, чтобы видеть новые логи
                _db.ChangeTracker.Clear();
                LogsGrid.ItemsSource = _logService.GetAllLogs();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка загрузки логов: " + ex.Message, "Ошибка", this);
            }
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as Button).DataContext is ActionLog log)) return;

            // Используем наше новое исправленное окно сообщений!
            bool confirm = CustomMessageBox.ShowQuestion(
                $"Вы уверены, что хотите отменить действие: {log.Details}?",
                "Подтверждение отмены", this);

            if (confirm)
            {
                try
                {
                    var result = _logService.UndoAction(log.Id);
                    if (result)
                    {
                        CustomMessageBox.Show("Действие успешно отменено!", "Успех", this);
                        LoadLogs();
                    }
                    else
                    {
                        CustomMessageBox.Show("Не удалось отменить действие. Возможно, оно уже было отменено ранее.", "Предупреждение", this);
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Критическая ошибка: {ex.Message}", "Ошибка", this);
                }
            }
        }
    }
}
