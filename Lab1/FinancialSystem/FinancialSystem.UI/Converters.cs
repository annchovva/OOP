using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media; // Добавьте для работы с цветами (SolidColorBrush)

namespace FinancialSystem.UI
{
    // --- ВАШИ СТАРЫЕ КОНВЕРТЕРЫ ---

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

    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "❌ Отменено" : "✅ Активно";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // --- НОВЫЕ КОНВЕРТЕРЫ (НУЖНЫ ДЛЯ ОКНА МЕНЕДЖЕРА) ---

    public class BlockStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBlocked)
                return isBlocked ? "ЗАБЛОКИРОВАН" : "АКТИВЕН";
            return "НЕИЗВЕСТНО";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BlockColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBlocked)
                // Возвращаем Красный для заблокированных и Изумрудный для активных
                return isBlocked ? new SolidColorBrush(Color.FromRgb(231, 76, 60)) : new SolidColorBrush(Color.FromRgb(46, 204, 113));
            return Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
