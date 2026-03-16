using System.Windows;
using System.Windows.Input;

namespace FinancialSystem.UI
{
    public partial class CustomMessageBox : Window
    {
        // приватный конструктор только через методы
        private CustomMessageBox(string message, string title, bool isQuestion)
        {
            InitializeComponent();
            MessageText.Text = message;

            this.MouseDown += (s, e) => {
                if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
            };

            // если вопрос, заменяем "ок" на "да/нет"
            if (isQuestion)
            {
                OkBtn.Visibility = Visibility.Collapsed;
                YesNoStack.Visibility = Visibility.Visible;
            }
        }

        public static void Show(string message, string title = "Уведомление", Window owner = null)
        {
            var msg = new CustomMessageBox(message, title, false);
            msg.Owner = owner ?? System.Windows.Application.Current.MainWindow;
            msg.WindowStartupLocation = WindowStartupLocation.CenterOwner; // обязательно по центру
            msg.ShowDialog();
        }

        public static bool ShowQuestion(string message, string title = "Подтверждение", Window owner = null)
        {
            var msg = new CustomMessageBox(message, title, true);
            msg.Owner = owner ?? System.Windows.Application.Current.MainWindow;
            msg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            return msg.ShowDialog() == true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; 
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; 
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; 
        }
    }
}
