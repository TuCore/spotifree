using Spotifree.Constances;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Spotifree.Converters;

public class RepeatModeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RepeatMode mode)
        {
            if (mode != RepeatMode.None)
            {
                return new SolidColorBrush(Colors.MediumSeaGreen);
            }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}