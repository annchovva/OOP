using System.Windows;

namespace FinancialSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ловим все необработанные ошибки
            this.DispatcherUnhandledException += (s, ex) =>
            {
                MessageBox.Show($"Произошла ошибка: {ex.Exception.Message}\n\n{ex.Exception.InnerException?.Message}", "Критическая ошибка");
                ex.Handled = true;
            };
        }
    }
}

