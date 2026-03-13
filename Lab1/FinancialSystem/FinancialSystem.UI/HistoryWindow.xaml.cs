using System.Windows;
using System.Collections.Generic;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.UI
{
    /// <summary>
    /// Логика взаимодействия для HistoryWindow.xaml
    /// </summary>
    public partial class HistoryWindow : Window
    {
        // Конструктор по умолчанию
        public HistoryWindow()
        {
            InitializeComponent();
        }

        // Вспомогательный метод для установки данных извне
        // (Мы вызываем его из DashboardWindow)
        public void SetHistoryData(List<TransactionRecord> history)
        {
            if (history == null || history.Count == 0)
            {
                MessageBox.Show("История операций пуста.");
            }

            HistoryGrid.ItemsSource = history;
        }
    }
}
