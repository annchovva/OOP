using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FinancialSystem.UI
{
    // Конвертер: если действие отменено (true), скрываем кнопку (Collapsed)
    public class InverseBoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // Конвертер: превращает true/false в текст "Отменено/Ок"
    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "❌ Отменено" : "✅ Активно";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

