using System.Globalization;
using System.Windows.Data;

namespace VlessVpnClient.App.Converters;

public sealed class ConnectionStatusTextConverter : IValueConverter
{
    public string ConnectedText { get; set; } = "Подключено";
    public string DisconnectedText { get; set; } = "Отключено";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && b ? ConnectedText : DisconnectedText;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
