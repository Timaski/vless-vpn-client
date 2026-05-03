using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VlessVpnClient.App.Converters;

public sealed class LatencyToBrushConverter : IValueConverter
{
    public Brush GoodBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    public Brush OkayBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07));
    public Brush BadBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    public Brush UnknownBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int ms)
        {
            return UnknownBrush;
        }
        return ms switch
        {
            < 0 => UnknownBrush,
            <= 200 => GoodBrush,
            <= 500 => OkayBrush,
            _ => BadBrush
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
