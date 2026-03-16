using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FinancialSystem.UI
{
    // 1. Скрывает кнопку, если действие уже отменено
    public class InverseBoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если IsReversed == true, возвращаем Collapsed (скрываем)
            if (value is bool isReversed && isReversed)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 2. Текст для логов администратора
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

    // 3. Текст для статуса блокировки счета (для окна менеджера)
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

    // 4. Цвет для статуса блокировки
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
