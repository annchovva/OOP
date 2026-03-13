using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FinancialSystem.Application.Interfaces;

namespace FinancialSystem.UI
{
    public partial class AccountHistoryWindow : Window
    {
        public AccountHistoryWindow(int accountId, IBankService bankService)
        {
            InitializeComponent();
            HistoryGrid.ItemsSource = bankService.GetAccountHistory(accountId);
        }
    }

}
