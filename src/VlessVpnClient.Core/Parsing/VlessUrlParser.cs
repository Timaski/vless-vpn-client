using System.Web;
using VlessVpnClient.Core.Models;

namespace VlessVpnClient.Core.Parsing;

/// <summary>
/// Parses <c>vless://uuid@host:port?...#name</c> URIs into <see cref="VlessConfig"/>.
/// </summary>
public static class VlessUrlParser
{
    public static bool TryParse(string url, out VlessConfig? config, out string? error)
    {
        config = null;
        error = null;

        if (string.IsNullOrWhiteSpace(url))
        {
            error = "URL is empty";
            return false;
        }

        var trimmed = url.Trim();
        if (!trimmed.StartsWith("vless://", StringComparison.OrdinalIgnoreCase))
        {
            error = "URL must start with vless://";
            return false;
        }

        try
        {
            var uri = new Uri(trimmed);

            var userInfo = Uri.UnescapeDataString(uri.UserInfo);
            if (string.IsNullOrEmpty(userInfo))
            {
                error = "Missing UUID in vless URL";
                return false;
            }

            var host = uri.IdnHost;
            if (string.IsNullOrEmpty(host))
            {
                error = "Missing host in vless URL";
                return false;
            }

            var port = uri.Port;
            if (port <= 0 || port > 65535)
            {
                error = "Invalid port in vless URL";
                return false;
            }

            var query = HttpUtility.ParseQueryString(uri.Query);
            string Get(string key) => query[key] ?? string.Empty;

            var remark = string.Empty;
            if (!string.IsNullOrEmpty(uri.Fragment))
            {
                remark = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'));
            }

            config = new VlessConfig
            {
                Id = userInfo,
                Address = host,
                Port = port,
                Encryption = string.IsNullOrEmpty(Get("encryption")) ? "none" : Get("encryption"),
                Flow = Get("flow"),
                Network = string.IsNullOrEmpty(Get("type")) ? "tcp" : Get("type"),
                Security = string.IsNullOrEmpty(Get("security")) ? "none" : Get("security"),
                Sni = Get("sni"),
                Alpn = Get("alpn"),
                Fingerprint = Get("fp"),
                PublicKey = Get("pbk"),
                ShortId = Get("sid"),
                SpiderX = Get("spx"),
                Path = Get("path"),
                Host = Get("host"),
                ServiceName = Get("serviceName"),
                Mode = Get("mode"),
                HeaderType = Get("headerType"),
                Seed = Get("seed"),
                Key = Get("key"),
                QuicSecurity = Get("quicSecurity"),
                Remark = remark
            };
            return true;
        }
        catch (Exception ex)
        {
            error = $"Failed to parse vless URL: {ex.Message}";
            return false;
        }
    }

    public static VlessConfig Parse(string url)
    {
        if (!TryParse(url, out var config, out var error) || config is null)
        {
            throw new FormatException(error ?? "Invalid vless URL");
        }
        return config;
    }
}
