using System.Windows;
using FinancialSystem.Infrastructure;

namespace FinancialSystem.WPF
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            using (var db = new FinanceDbContext())
            {
                DbInitializer.Initialize(db);
            }
        }
    }
}


