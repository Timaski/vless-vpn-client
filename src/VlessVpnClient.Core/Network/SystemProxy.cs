using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace VlessVpnClient.Core.Network;

/// <summary>
/// Toggles Windows system-wide HTTP proxy via the registry and broadcasts a WinINet
/// settings change so all running apps pick it up immediately.
/// </summary>
[SupportedOSPlatform("windows")]
public static class SystemProxy
{
    private const string InternetSettingsKey =
        @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

    private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
    private const int INTERNET_OPTION_REFRESH = 37;

    [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

    public static void Enable(string host, int port, IEnumerable<string>? bypassList = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(InternetSettingsKey, writable: true)
            ?? throw new InvalidOperationException("Cannot open Internet Settings registry key");

        key.SetValue("ProxyEnable", 1, Microsoft.Win32.RegistryValueKind.DWord);
        key.SetValue("ProxyServer", $"{host}:{port}", Microsoft.Win32.RegistryValueKind.String);

        if (bypassList is not null)
        {
            var list = string.Join(';', bypassList);
            if (!string.IsNullOrEmpty(list))
            {
                key.SetValue("ProxyOverride", list, Microsoft.Win32.RegistryValueKind.String);
            }
        }

        Refresh();
    }

    public static void Disable()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(InternetSettingsKey, writable: true);
        if (key is null) return;
        key.SetValue("ProxyEnable", 0, Microsoft.Win32.RegistryValueKind.DWord);
        Refresh();
    }

    public static (bool Enabled, string? Server) GetCurrent()
    {
        if (!OperatingSystem.IsWindows())
        {
            return (false, null);
        }

        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(InternetSettingsKey, writable: false);
        if (key is null) return (false, null);
        var enabled = (key.GetValue("ProxyEnable") as int?) == 1;
        var server = key.GetValue("ProxyServer") as string;
        return (enabled, server);
    }

    private static void Refresh()
    {
        try
        {
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
        catch
        {
            // best-effort
        }
    }

    public static IEnumerable<string> DefaultBypassList() => new[]
    {
        "localhost",
        "127.*",
        "10.*",
        "172.16.*",
        "172.17.*",
        "172.18.*",
        "172.19.*",
        "172.20.*",
        "172.21.*",
        "172.22.*",
        "172.23.*",
        "172.24.*",
        "172.25.*",
        "172.26.*",
        "172.27.*",
        "172.28.*",
        "172.29.*",
        "172.30.*",
        "172.31.*",
        "192.168.*",
        "<local>"
    };
}
