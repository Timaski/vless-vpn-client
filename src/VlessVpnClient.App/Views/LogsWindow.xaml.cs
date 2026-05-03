using System.Diagnostics;
using System.Windows;
using VlessVpnClient.Core.Storage;

namespace VlessVpnClient.App.Views;

public partial class LogsWindow : Window
{
    public LogsWindow()
    {
        InitializeComponent();
        LogBox.Text = string.Join(Environment.NewLine, App.Logger.Snapshot());
        App.Logger.EntryAdded += OnEntry;
        Closed += (_, _) => App.Logger.EntryAdded -= OnEntry;
    }

    private void OnEntry(object? sender, string e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            LogBox.AppendText(e + Environment.NewLine);
            LogBox.ScrollToEnd();
        });
    }

    private void OpenFolder_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = AppPaths.LogsDirectory,
                UseShellExecute = true
            });
        }
        catch { /* ignore */ }
    }

    private void Clear_Click(object? sender, RoutedEventArgs e)
    {
        LogBox.Clear();
    }
}
