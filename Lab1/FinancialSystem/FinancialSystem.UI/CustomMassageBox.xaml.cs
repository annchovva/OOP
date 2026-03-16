using System.Windows;
using System.Windows.Input;

namespace FinancialSystem.UI
{
    public partial class CustomMessageBox : Window
    {
        // Конструктор теперь приватный, используем только через статические методы
        private CustomMessageBox(string message, string title, bool isQuestion)
        {
            InitializeComponent();
            MessageText.Text = message;

            // Добавляем возможность перетаскивать окно за любое место
            this.MouseDown += (s, e) => {
                if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
            };

            if (isQuestion)
            {
                OkBtn.Visibility = Visibility.Collapsed;
                YesNoStack.Visibility = Visibility.Visible;
            }
        }

        // 1. Обычное уведомление
        public static void Show(string message, string title = "Уведомление", Window owner = null)
        {
            var msg = new CustomMessageBox(message, title, false);
            // Важная проверка на наличие владельца
            msg.Owner = owner ?? System.Windows.Application.Current.MainWindow;
            msg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            msg.ShowDialog();
        }

        // 2. Метод для вопросов (Возвращает true если "Да")
        public static bool ShowQuestion(string message, string title = "Подтверждение", Window owner = null)
        {
            var msg = new CustomMessageBox(message, title, true);
            msg.Owner = owner ?? System.Windows.Application.Current.MainWindow;
            msg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // ShowDialog возвращает nullable bool. Мы приводим его к обычному bool.
            return msg.ShowDialog() == true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Это автоматически закроет окно
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Результат true
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Результат false
        }
    }
}
