using System.Windows;
using FinancialSystem.Application.Interfaces;

namespace FinancialSystem.UI
{
    public partial class AccountHistoryWindow : Window
    {
        public AccountHistoryWindow(int accountId, IBankService bankService)
        {
            InitializeComponent();
            // Загружаем данные в таблицу
            HistoryGrid.ItemsSource = bankService.GetAccountHistory(accountId);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
