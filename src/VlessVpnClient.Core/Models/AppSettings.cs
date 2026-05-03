namespace VlessVpnClient.Core.Models;

public enum ProxyMode
{
    /// <summary>HTTP/SOCKS proxy on localhost; system proxy gets pointed at it.</summary>
    SystemProxy,
    /// <summary>Inbound only — user configures their own apps to use the proxy.</summary>
    Manual
}

public enum AppTheme
{
    System,
    Light,
    Dark
}

public sealed class AppSettings
{
    public ProxyMode ProxyMode { get; set; } = ProxyMode.SystemProxy;
    public AppTheme Theme { get; set; } = AppTheme.Dark;

    public int HttpInboundPort { get; set; } = 10809;
    public int SocksInboundPort { get; set; } = 10808;
    public bool EnableHttpInbound { get; set; } = true;
    public bool EnableSocksInbound { get; set; } = true;

    public bool BypassLan { get; set; } = true;
    public bool AutoStart { get; set; } = false;
    public bool StartMinimized { get; set; } = false;
    public bool ConnectOnStart { get; set; } = false;

    public string? ActiveProfileId { get; set; }
    public string? XrayBinaryPath { get; set; }

    public string LogLevel { get; set; } = "warning";
    public string PingTestUrl { get; set; } = "https://www.gstatic.com/generate_204";
    public int PingTimeoutMs { get; set; } = 5000;

    public string DefaultSubscriptionUserAgent { get; set; } = "v2rayN/6.0";

    public string Language { get; set; } = "ru";
}
