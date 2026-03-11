using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinancialSystem.Domain.Entities;
using FinancialSystem.Domain.Enums;
using FinancialSystem.Infrastructure;

namespace FinancialSystem.UI
{
    public partial class ManagerWindow : Window
    {
        private readonly FinanceDbContext _db;

        public ManagerWindow()
        {
            InitializeComponent();
            _db = new FinanceDbContext();
            LoadPendingUsers();
        }

        private void LoadPendingUsers()
        {
            // Берем всех пользователей со статусом Pending
            var users = _db.Users.Where(u => u.Status == UserStatus.Pending).ToList();
            PendingUsersGrid.ItemsSource = users;
        }

        private void ApproveUser_Click(object sender, RoutedEventArgs e)
        {
            // Достаем пользователя из строки таблицы
            var button = sender as Button;
            var user = button?.DataContext as User;

            if (user != null)
            {
                var userInDb = _db.Users.Find(user.Id);
                if (userInDb != null)
                {
                    userInDb.Status = UserStatus.Active;
                    userInDb.IsApproved = true; // Для совместимости с твоим AuthService
                    _db.SaveChanges();

                    MessageBox.Show($"Пользователь {userInDb.Login} одобрен!");
                    LoadPendingUsers(); // Обновляем список
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
