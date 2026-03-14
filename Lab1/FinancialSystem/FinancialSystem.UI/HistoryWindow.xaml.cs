using System.Windows;
using System.Collections.Generic;
using FinancialSystem.Domain.Entities;
using System.Linq;

namespace FinancialSystem.UI
{
    public partial class HistoryWindow : Window
    {
        public HistoryWindow()
        {
            InitializeComponent();
        }

        public void SetHistoryData(List<TransactionRecord> history)
        {
            if (history == null || !history.Any())
            {
                // Вместо простого MessageBox можно вывести текст прямо в окне, 
                // но пока оставим логику для простоты
                HistoryGrid.ItemsSource = null;
                return;
            }

            HistoryGrid.ItemsSource = history;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
