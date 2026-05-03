using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using VlessVpnClient.Core.Logging;

namespace VlessVpnClient.Core.Xray;

/// <summary>
/// Downloads the latest xray-core release from GitHub for the current OS/arch.
/// </summary>
public sealed class XrayDownloader
{
    private const string LatestReleaseApi = "https://api.github.com/repos/XTLS/Xray-core/releases/latest";
    private readonly IAppLogger _logger;
    private readonly HttpClient _http;

    public XrayDownloader(IAppLogger logger, HttpClient? http = null)
    {
        _logger = logger;
        _http = http ?? CreateClient();
    }

    private static HttpClient CreateClient()
    {
        var c = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5)
        };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("VlessVpnClient/1.0");
        c.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return c;
    }

    /// <summary>
    /// Returns the path to the xray binary inside <paramref name="targetDirectory"/>.
    /// </summary>
    public async Task<string> EnsureXrayAsync(string targetDirectory, CancellationToken ct = default)
    {
        Directory.CreateDirectory(targetDirectory);
        var binaryName = OperatingSystem.IsWindows() ? "xray.exe" : "xray";
        var existing = Path.Combine(targetDirectory, binaryName);
        if (File.Exists(existing))
        {
            _logger.LogInfo($"xray already present at {existing}");
            return existing;
        }

        _logger.LogInfo("Resolving latest xray-core release from GitHub");
        using var releaseResp = await _http.GetAsync(LatestReleaseApi, ct).ConfigureAwait(false);
        releaseResp.EnsureSuccessStatusCode();
        var releaseJson = await releaseResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        using var doc = JsonDocument.Parse(releaseJson);

        var assetSuffix = ResolveAssetSuffix();
        var assets = doc.RootElement.GetProperty("assets");
        string? downloadUrl = null;
        string? assetName = null;
        foreach (var asset in assets.EnumerateArray())
        {
            var name = asset.GetProperty("name").GetString();
            if (name is null) continue;
            if (name.EndsWith(assetSuffix, StringComparison.OrdinalIgnoreCase))
            {
                downloadUrl = asset.GetProperty("browser_download_url").GetString();
                assetName = name;
                break;
            }
        }

        if (downloadUrl is null)
        {
            throw new InvalidOperationException(
                $"Could not find a release asset matching {assetSuffix} in xray-core latest release");
        }

        _logger.LogInfo($"Downloading {assetName} from {downloadUrl}");

        var zipPath = Path.Combine(targetDirectory, assetName!);
        await using (var src = await _http.GetStreamAsync(downloadUrl, ct).ConfigureAwait(false))
        await using (var dst = File.Create(zipPath))
        {
            await src.CopyToAsync(dst, ct).ConfigureAwait(false);
        }

        _logger.LogInfo($"Extracting {zipPath} to {targetDirectory}");
        ZipFile.ExtractToDirectory(zipPath, targetDirectory, overwriteFiles: true);
        try { File.Delete(zipPath); } catch { /* best-effort */ }

        if (!File.Exists(existing))
        {
            throw new InvalidOperationException(
                $"Expected xray binary at {existing} after extraction; not found");
        }

        if (!OperatingSystem.IsWindows())
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("chmod", $"+x \"{existing}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                System.Diagnostics.Process.Start(psi)?.WaitForExit();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not chmod xray binary: {ex.Message}");
            }
        }

        _logger.LogInfo($"xray ready at {existing}");
        return existing;
    }

    private static string ResolveAssetSuffix()
    {
        if (OperatingSystem.IsWindows())
        {
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.X64 => "windows-64.zip",
                Architecture.X86 => "windows-32.zip",
                Architecture.Arm64 => "windows-arm64-v8a.zip",
                _ => "windows-64.zip"
            };
        }
        if (OperatingSystem.IsLinux())
        {
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.X64 => "linux-64.zip",
                Architecture.Arm64 => "linux-arm64-v8a.zip",
                _ => "linux-64.zip"
            };
        }
        if (OperatingSystem.IsMacOS())
        {
            return RuntimeInformation.OSArchitecture == Architecture.Arm64
                ? "macos-arm64-v8a.zip"
                : "macos-64.zip";
        }
        return "windows-64.zip";
    }
}
