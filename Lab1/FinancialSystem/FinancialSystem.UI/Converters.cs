using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FinancialSystem.UI
{
    // сокрытие кнопки "отмена" после отмены действия
    public class InverseBoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isReversed && isReversed)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // статус логов из bool в надпись
    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isReversed)
                return isReversed ? "❌ Отменено" : "✅ Активно";
            return "Неизвестно";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // статус блокировки
    public class BlockStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBlocked)
                return isBlocked ? "⛔ ЗАБЛОКИРОВАН" : "💳 АКТИВЕН";
            return "НЕИЗВЕСТНО";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BlockColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBlocked)
            {
                // Используем цвета из вашей палитры (Красный vs Изумрудный)
                return isBlocked
                    ? new SolidColorBrush(Color.FromRgb(231, 76, 60))  // #E74C3C
                    : new SolidColorBrush(Color.FromRgb(46, 204, 113)); // #2ECC71
            }
            return Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
