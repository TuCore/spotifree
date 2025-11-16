using MaterialDesignThemes.Wpf;
using System.Globalization;
using System.Windows.Data;

namespace Spotifree.Converters;

public class PlayPauseIconKindConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? PackIconKind.Pause : PackIconKind.Play;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}