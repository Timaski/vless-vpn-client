using System.Threading;
using System.Windows;
using VlessVpnClient.App.Services;
using VlessVpnClient.App.ViewModels;
using VlessVpnClient.Core.Logging;
using VlessVpnClient.Core.Storage;

namespace VlessVpnClient.App;

public partial class App : Application
{
    private static Mutex? _singleInstanceMutex;

    public static FileLogger Logger { get; private set; } = null!;
    public static SettingsStore SettingsStore { get; private set; } = null!;
    public static ProfileStore ProfileStore { get; private set; } = null!;
    public static SubscriptionStore SubscriptionStore { get; private set; } = null!;
    public static ConnectionManager ConnectionManager { get; private set; } = null!;
    public static MainViewModel MainVm { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        bool createdNew;
        _singleInstanceMutex = new Mutex(true, "Global\\VlessVpnClient.SingleInstance", out createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Приложение уже запущено.", "VLESS VPN Client",
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        AppPaths.EnsureDirectories();

        Logger = new FileLogger(AppPaths.LogFilePath);
        Logger.LogInfo($"VLESS VPN Client started, command line: {string.Join(' ', e.Args)}");

        SettingsStore = new SettingsStore(AppPaths.SettingsPath);
        ProfileStore = new ProfileStore(AppPaths.ProfilesPath);
        SubscriptionStore = new SubscriptionStore(AppPaths.SubscriptionsPath);

        ThemeManager.Apply(SettingsStore.Current.Theme);

        ConnectionManager = new ConnectionManager(SettingsStore, Logger);

        MainVm = new MainViewModel(ProfileStore, SubscriptionStore, SettingsStore, ConnectionManager, Logger);

        DispatcherUnhandledException += (_, args) =>
        {
            Logger.LogError($"UI exception: {args.Exception}");
            MessageBox.Show(args.Exception.Message, "VLESS VPN Client — ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        var startMinimized = e.Args.Any(a =>
            string.Equals(a, "--minimized", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(a, "-m", StringComparison.OrdinalIgnoreCase))
            || SettingsStore.Current.StartMinimized;

        var window = new MainWindow();
        if (startMinimized)
        {
            window.WindowState = WindowState.Minimized;
            window.ShowInTaskbar = false;
        }
        window.Show();

        if (SettingsStore.Current.ConnectOnStart && SettingsStore.Current.ActiveProfileId is not null)
        {
            var active = ProfileStore.GetById(SettingsStore.Current.ActiveProfileId);
            if (active is not null)
            {
                _ = ConnectionManager.ConnectAsync(active);
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            ConnectionManager?.Dispose();
        }
        catch { /* ignore */ }
        Logger?.LogInfo("VLESS VPN Client exiting");
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
