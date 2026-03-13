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
    public partial class TransferWindow : Window
    {
        private readonly int _fromId;
        private readonly IBankService _bankService;

        public TransferWindow(int fromId, IBankService bankService)
        {
            InitializeComponent();
            _fromId = fromId;
            _bankService = bankService;
        }

        private void Transfer_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                bool success = _bankService.TransferMoney(_fromId, ToAccountTextBox.Text, amount);
                if (success) { MessageBox.Show("Успешно!"); DialogResult = true; }
                else MessageBox.Show("Ошибка: проверьте баланс или номер счета.");
            }
        }
    }

}
