namespace VlessVpnClient.Core.Storage;

/// <summary>
/// Resolves the on-disk locations used by the app. Everything goes under
/// <c>%LOCALAPPDATA%\VlessVpnClient</c> on Windows; XDG-style on other OSes.
/// </summary>
public static class AppPaths
{
    public const string AppFolderName = "VlessVpnClient";

    public static string DataDirectory
    {
        get
        {
            var baseDir = OperatingSystem.IsWindows()
                ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                : Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".local", "share");
            return Path.Combine(baseDir, AppFolderName);
        }
    }

    public static string XrayDirectory => Path.Combine(DataDirectory, "xray");
    public static string LogsDirectory => Path.Combine(DataDirectory, "logs");
    public static string ProfilesPath => Path.Combine(DataDirectory, "profiles.json");
    public static string SettingsPath => Path.Combine(DataDirectory, "settings.json");
    public static string SubscriptionsPath => Path.Combine(DataDirectory, "subscriptions.json");
    public static string ActiveConfigPath => Path.Combine(DataDirectory, "config.json");
    public static string LogFilePath => Path.Combine(LogsDirectory, "vless-vpn-client.log");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(XrayDirectory);
        Directory.CreateDirectory(LogsDirectory);
    }
}
