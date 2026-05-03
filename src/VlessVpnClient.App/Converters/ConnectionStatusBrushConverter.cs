using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VlessVpnClient.App.Converters;

public sealed class ConnectionStatusBrushConverter : IValueConverter
{
    public Brush ConnectedBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0x23, 0xA5, 0x59));
    public Brush DisconnectedBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? ConnectedBrush : DisconnectedBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
