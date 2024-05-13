using System.Globalization;
using System.Windows.Data;

namespace LedOperationSample.Converters;

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum)
        {
            return value.ToString();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string)
        {
            return Enum.Parse(targetType, (string)value);
        }
        return value;
    }
}
