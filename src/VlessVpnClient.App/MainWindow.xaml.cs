using System.Windows;
using System.Windows.Input;
using VlessVpnClient.App.Services;
using VlessVpnClient.App.Views;

namespace VlessVpnClient.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.MainVm;
        StateChanged += OnStateChanged;
        Closing += OnClosing;
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            ShowInTaskbar = false;
            Hide();
        }
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Hide to tray instead of exiting; user can right-click tray > Выход.
        if (App.ConnectionManager?.IsConnected == true)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            Hide();
        }
    }

    private void Tray_MouseDoubleClick(object? sender, RoutedEventArgs e)
    {
        ShowFromTray();
    }

    private void MenuShow_Click(object? sender, RoutedEventArgs e) => ShowFromTray();

    private void ShowFromTray()
    {
        Show();
        ShowInTaskbar = true;
        WindowState = WindowState.Normal;
        Activate();
    }

    private void MenuConnect_Click(object? sender, RoutedEventArgs e)
    {
        if (App.MainVm.SelectedProfile is null && App.MainVm.Profiles.Count > 0)
        {
            App.MainVm.SelectedProfile = App.MainVm.Profiles[0];
        }
        if (App.MainVm.ConnectCommand.CanExecute(null))
        {
            App.MainVm.ConnectCommand.Execute(null);
        }
    }

    private void MenuDisconnect_Click(object? sender, RoutedEventArgs e)
    {
        if (App.MainVm.DisconnectCommand.CanExecute(null))
        {
            App.MainVm.DisconnectCommand.Execute(null);
        }
    }

    private void MenuExit_Click(object? sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private async void OnImportClick(object? sender, RoutedEventArgs e)
    {
        var dlg = new ImportDialog { Owner = this };
        var result = dlg.ShowDialog();
        if (result == true)
        {
            await dlg.ProcessImportAsync();
        }
    }

    private void OnSubscriptionsClick(object? sender, RoutedEventArgs e)
    {
        var dlg = new SubscriptionsWindow { Owner = this };
        dlg.ShowDialog();
    }

    private void OnLogsClick(object? sender, RoutedEventArgs e)
    {
        var dlg = new LogsWindow { Owner = this };
        dlg.Show();
    }

    private void OnSettingsClick(object? sender, RoutedEventArgs e)
    {
        var dlg = new SettingsWindow { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            ThemeManager.Apply(App.SettingsStore.Current.Theme);
            if (OperatingSystem.IsWindows())
            {
                if (App.SettingsStore.Current.AutoStart)
                {
                    AutoStartManager.Enable(App.SettingsStore.Current.StartMinimized);
                }
                else
                {
                    AutoStartManager.Disable();
                }
            }
        }
    }

    private void OnSortByLatencyClick(object? sender, RoutedEventArgs e)
    {
        App.MainVm.SortByLatency();
    }

    private void ProfilesGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (App.MainVm.SelectedProfile is null) return;
        if (App.MainVm.ConnectCommand.CanExecute(null))
        {
            App.MainVm.ConnectCommand.Execute(null);
        }
    }
}
