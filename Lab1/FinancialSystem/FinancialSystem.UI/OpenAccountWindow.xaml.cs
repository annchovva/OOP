using System.Collections.Generic;
using System.Windows;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.UI
{
    public partial class OpenAccountWindow : Window
    {
        public Bank SelectedBank { get; private set; }

        public OpenAccountWindow(List<Bank> banks)
        {
            InitializeComponent();
            BankComboBox.ItemsSource = banks;
            if (banks.Count > 0) BankComboBox.SelectedIndex = 0;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            SelectedBank = BankComboBox.SelectedItem as Bank;
            if (SelectedBank != null)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
