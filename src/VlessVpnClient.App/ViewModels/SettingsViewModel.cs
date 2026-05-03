using VlessVpnClient.App.Helpers;
using VlessVpnClient.Core.Models;

namespace VlessVpnClient.App.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    public AppSettings Settings { get; }

    public SettingsViewModel(AppSettings settings)
    {
        Settings = settings;
        _httpPort = settings.HttpInboundPort;
        _socksPort = settings.SocksInboundPort;
        _enableHttp = settings.EnableHttpInbound;
        _enableSocks = settings.EnableSocksInbound;
        _bypassLan = settings.BypassLan;
        _autoStart = settings.AutoStart;
        _startMinimized = settings.StartMinimized;
        _connectOnStart = settings.ConnectOnStart;
        _logLevel = settings.LogLevel;
        _pingTestUrl = settings.PingTestUrl;
        _pingTimeoutMs = settings.PingTimeoutMs;
        _theme = settings.Theme;
        _proxyMode = settings.ProxyMode;
        _userAgent = settings.DefaultSubscriptionUserAgent;
    }

    private int _httpPort;
    public int HttpInboundPort
    {
        get => _httpPort;
        set => SetField(ref _httpPort, value);
    }

    private int _socksPort;
    public int SocksInboundPort
    {
        get => _socksPort;
        set => SetField(ref _socksPort, value);
    }

    private bool _enableHttp;
    public bool EnableHttpInbound
    {
        get => _enableHttp;
        set => SetField(ref _enableHttp, value);
    }

    private bool _enableSocks;
    public bool EnableSocksInbound
    {
        get => _enableSocks;
        set => SetField(ref _enableSocks, value);
    }

    private bool _bypassLan;
    public bool BypassLan
    {
        get => _bypassLan;
        set => SetField(ref _bypassLan, value);
    }

    private bool _autoStart;
    public bool AutoStart
    {
        get => _autoStart;
        set => SetField(ref _autoStart, value);
    }

    private bool _startMinimized;
    public bool StartMinimized
    {
        get => _startMinimized;
        set => SetField(ref _startMinimized, value);
    }

    private bool _connectOnStart;
    public bool ConnectOnStart
    {
        get => _connectOnStart;
        set => SetField(ref _connectOnStart, value);
    }

    private string _logLevel = "warning";
    public string LogLevel
    {
        get => _logLevel;
        set => SetField(ref _logLevel, value);
    }

    private string _pingTestUrl = string.Empty;
    public string PingTestUrl
    {
        get => _pingTestUrl;
        set => SetField(ref _pingTestUrl, value);
    }

    private int _pingTimeoutMs;
    public int PingTimeoutMs
    {
        get => _pingTimeoutMs;
        set => SetField(ref _pingTimeoutMs, value);
    }

    private AppTheme _theme;
    public AppTheme Theme
    {
        get => _theme;
        set => SetField(ref _theme, value);
    }

    private ProxyMode _proxyMode;
    public ProxyMode ProxyMode
    {
        get => _proxyMode;
        set => SetField(ref _proxyMode, value);
    }

    private string _userAgent = string.Empty;
    public string UserAgent
    {
        get => _userAgent;
        set => SetField(ref _userAgent, value);
    }

    public void Apply()
    {
        Settings.HttpInboundPort = HttpInboundPort;
        Settings.SocksInboundPort = SocksInboundPort;
        Settings.EnableHttpInbound = EnableHttpInbound;
        Settings.EnableSocksInbound = EnableSocksInbound;
        Settings.BypassLan = BypassLan;
        Settings.AutoStart = AutoStart;
        Settings.StartMinimized = StartMinimized;
        Settings.ConnectOnStart = ConnectOnStart;
        Settings.LogLevel = LogLevel;
        Settings.PingTestUrl = PingTestUrl;
        Settings.PingTimeoutMs = PingTimeoutMs;
        Settings.Theme = Theme;
        Settings.ProxyMode = ProxyMode;
        Settings.DefaultSubscriptionUserAgent = UserAgent;
    }
}
