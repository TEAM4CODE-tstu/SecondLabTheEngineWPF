using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GoodsLogistics.View
{
    public class HighlightToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isHighlighted && isHighlighted)
                return new SolidColorBrush(Color.FromRgb(255, 250, 205)); // Жёлтый
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}