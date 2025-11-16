using MaterialDesignThemes.Wpf;
using Spotifree.Constances;
using System.Globalization;
using System.Windows.Data;

namespace Spotifree.Converters;

public class RepeatModeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RepeatMode mode)
        {
            return mode switch
            {
                RepeatMode.RepeatOne => PackIconKind.RepeatOnce,
                RepeatMode.RepeatAll => PackIconKind.Repeat,
                _ => PackIconKind.RepeatOff
            };
        }
        return PackIconKind.RepeatOff;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}