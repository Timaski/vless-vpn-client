using System.Windows;
using VlessVpnClient.Core.Models;

namespace VlessVpnClient.App.Services;

public static class ThemeManager
{
    public static void Apply(AppTheme theme)
    {
        var resources = Application.Current.Resources;

        var existing = resources.MergedDictionaries
            .Where(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"))
            .ToList();
        foreach (var d in existing)
        {
            resources.MergedDictionaries.Remove(d);
        }

        var path = theme switch
        {
            AppTheme.Light => "/Themes/LightTheme.xaml",
            _ => "/Themes/DarkTheme.xaml"
        };

        var dict = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/VlessVpnClient;component{path}", UriKind.Absolute)
        };
        resources.MergedDictionaries.Insert(0, dict);
    }
}
