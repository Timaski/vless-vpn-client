using System.Windows;
using VlessVpnClient.Core.Models;

namespace VlessVpnClient.App.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Load();
    }

    private void Load()
    {
        var s = App.SettingsStore.Current;
        ProxyModeBox.SelectedIndex = s.ProxyMode == ProxyMode.SystemProxy ? 0 : 1;
        EnableHttpBox.IsChecked = s.EnableHttpInbound;
        EnableSocksBox.IsChecked = s.EnableSocksInbound;
        HttpPortBox.Text = s.HttpInboundPort.ToString();
        SocksPortBox.Text = s.SocksInboundPort.ToString();
        BypassLanBox.IsChecked = s.BypassLan;
        AutoStartBox.IsChecked = s.AutoStart;
        StartMinimizedBox.IsChecked = s.StartMinimized;
        ConnectOnStartBox.IsChecked = s.ConnectOnStart;

        ThemeBox.SelectedIndex = s.Theme switch
        {
            AppTheme.Light => 1,
            _ => 0
        };

        LogLevelBox.SelectedIndex = s.LogLevel switch
        {
            "debug" => 0,
            "info" => 1,
            "warning" => 2,
            "error" => 3,
            "none" => 4,
            _ => 2
        };

        PingUrlBox.Text = s.PingTestUrl;
        PingTimeoutBox.Text = s.PingTimeoutMs.ToString();
        UserAgentBox.Text = s.DefaultSubscriptionUserAgent;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        App.SettingsStore.Update(s =>
        {
            s.ProxyMode = ProxyModeBox.SelectedIndex == 0 ? ProxyMode.SystemProxy : ProxyMode.Manual;
            s.EnableHttpInbound = EnableHttpBox.IsChecked == true;
            s.EnableSocksInbound = EnableSocksBox.IsChecked == true;
            if (int.TryParse(HttpPortBox.Text, out var hp)) s.HttpInboundPort = hp;
            if (int.TryParse(SocksPortBox.Text, out var sp)) s.SocksInboundPort = sp;
            s.BypassLan = BypassLanBox.IsChecked == true;
            s.AutoStart = AutoStartBox.IsChecked == true;
            s.StartMinimized = StartMinimizedBox.IsChecked == true;
            s.ConnectOnStart = ConnectOnStartBox.IsChecked == true;
            s.Theme = ThemeBox.SelectedIndex == 1 ? AppTheme.Light : AppTheme.Dark;
            s.LogLevel = LogLevelBox.SelectedIndex switch
            {
                0 => "debug",
                1 => "info",
                2 => "warning",
                3 => "error",
                4 => "none",
                _ => "warning"
            };
            s.PingTestUrl = PingUrlBox.Text?.Trim() ?? s.PingTestUrl;
            if (int.TryParse(PingTimeoutBox.Text, out var pt)) s.PingTimeoutMs = pt;
            s.DefaultSubscriptionUserAgent = UserAgentBox.Text?.Trim() ?? s.DefaultSubscriptionUserAgent;
        });

        DialogResult = true;
        Close();
    }
}
