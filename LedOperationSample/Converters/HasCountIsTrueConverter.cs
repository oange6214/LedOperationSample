using System.Globalization;
using System.Windows.Data;

namespace LedOperationSample.Converters;

public class HasCountIsTrueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int integerValue)
        {
            return integerValue == 0;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}