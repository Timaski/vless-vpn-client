using System.IO;
using System.Runtime.Versioning;
using VlessVpnClient.App.Helpers;
using VlessVpnClient.Core.Logging;
using VlessVpnClient.Core.Models;
using VlessVpnClient.Core.Network;
using VlessVpnClient.Core.Storage;
using VlessVpnClient.Core.Xray;

namespace VlessVpnClient.App.Services;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

public sealed class ConnectionManager : ObservableObject, IDisposable
{
    private readonly XrayProcessManager _xray;
    private readonly XrayDownloader _downloader;
    private readonly IAppLogger _logger;
    private readonly SettingsStore _settingsStore;

    private ConnectionState _state = ConnectionState.Disconnected;
    private ServerProfile? _activeProfile;
    private string? _statusMessage;

    public ConnectionState State
    {
        get => _state;
        private set
        {
            if (SetField(ref _state, value))
            {
                RaisePropertyChanged(nameof(IsConnected));
                RaisePropertyChanged(nameof(IsBusy));
            }
        }
    }

    public bool IsConnected => _state == ConnectionState.Connected;
    public bool IsBusy => _state == ConnectionState.Connecting;

    public ServerProfile? ActiveProfile
    {
        get => _activeProfile;
        private set => SetField(ref _activeProfile, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public event EventHandler? StateChanged;

    public ConnectionManager(SettingsStore settingsStore, IAppLogger logger)
    {
        _logger = logger;
        _settingsStore = settingsStore;
        _xray = new XrayProcessManager(logger);
        _downloader = new XrayDownloader(logger);

        _xray.Exited += (_, code) =>
        {
            if (_state == ConnectionState.Connected)
            {
                StatusMessage = $"xray exited unexpectedly (code {code})";
                State = ConnectionState.Error;
                StateChanged?.Invoke(this, EventArgs.Empty);
                DisableSystemProxySafe();
            }
        };
    }

    public async Task ConnectAsync(ServerProfile profile, CancellationToken ct = default)
    {
        if (_state == ConnectionState.Connecting) return;

        try
        {
            State = ConnectionState.Connecting;
            ActiveProfile = profile;
            StatusMessage = "Подготавливаем xray-core…";

            AppPaths.EnsureDirectories();
            var binaryPath = await ResolveXrayBinaryAsync(ct).ConfigureAwait(true);
            _settingsStore.Update(s => s.XrayBinaryPath = binaryPath);

            StatusMessage = "Запускаем xray…";
            _xray.Start(binaryPath, profile, _settingsStore.Current, AppPaths.XrayDirectory);

            await Task.Delay(700, ct).ConfigureAwait(true);

            if (OperatingSystem.IsWindows() && _settingsStore.Current.ProxyMode == ProxyMode.SystemProxy)
            {
                EnableSystemProxy(_settingsStore.Current);
            }

            _settingsStore.Update(s => s.ActiveProfileId = profile.ProfileId);
            StatusMessage = $"Подключено: {profile.DisplayName}";
            State = ConnectionState.Connected;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connection failed: {ex}");
            StatusMessage = $"Ошибка подключения: {ex.Message}";
            State = ConnectionState.Error;
            DisableSystemProxySafe();
            try { _xray.Stop(); } catch { /* ignore */ }
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Task DisconnectAsync()
    {
        try
        {
            DisableSystemProxySafe();
            _xray.Stop();
            StatusMessage = "Отключено";
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error during disconnect: {ex.Message}");
        }
        finally
        {
            State = ConnectionState.Disconnected;
            ActiveProfile = null;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        return Task.CompletedTask;
    }

    private async Task<string> ResolveXrayBinaryAsync(CancellationToken ct)
    {
        var configured = _settingsStore.Current.XrayBinaryPath;
        if (!string.IsNullOrEmpty(configured) && File.Exists(configured))
        {
            return configured;
        }

        var bundled = TryLocateBundledXray();
        if (bundled is not null)
        {
            return bundled;
        }

        return await _downloader.EnsureXrayAsync(AppPaths.XrayDirectory, ct).ConfigureAwait(true);
    }

    private static string? TryLocateBundledXray()
    {
        var dir = AppContext.BaseDirectory;
        var candidates = OperatingSystem.IsWindows()
            ? new[]
            {
                Path.Combine(dir, "xray", "xray.exe"),
                Path.Combine(dir, "xray.exe")
            }
            : new[]
            {
                Path.Combine(dir, "xray", "xray"),
                Path.Combine(dir, "xray")
            };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }
        return null;
    }

    [SupportedOSPlatformGuard("windows")]
    private static void EnableSystemProxy(AppSettings settings)
    {
        if (!OperatingSystem.IsWindows()) return;

        var port = settings.EnableHttpInbound
            ? settings.HttpInboundPort
            : settings.SocksInboundPort;

        var bypass = settings.BypassLan
            ? SystemProxy.DefaultBypassList()
            : null;

        SystemProxy.Enable("127.0.0.1", port, bypass);
    }

    private static void DisableSystemProxySafe()
    {
        if (!OperatingSystem.IsWindows()) return;
        try { SystemProxy.Disable(); } catch { /* ignore */ }
    }

    public void Dispose()
    {
        DisableSystemProxySafe();
        _xray.Dispose();
    }
}
