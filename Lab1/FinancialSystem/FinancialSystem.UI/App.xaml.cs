using System.Windows;
using FinancialSystem.Infrastructure;

namespace FinancialSystem.WPF
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Инициализируем базу данных (создаем файл и таблицы, если их нет)
            using (var db = new FinanceDbContext())
            {
                // Это создаст таблицы и добавит начальных пользователей (админа/менеджера)
                DbInitializer.Initialize(db);
            }
        }
    }
}


