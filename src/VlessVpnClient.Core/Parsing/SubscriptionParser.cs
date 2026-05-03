using System.Text;
using VlessVpnClient.Core.Models;

namespace VlessVpnClient.Core.Parsing;

/// <summary>
/// Decodes subscription bodies (base64 or plain text) into VLESS configs.
/// </summary>
public static class SubscriptionParser
{
    public static List<VlessConfig> Parse(string body)
    {
        var results = new List<VlessConfig>();
        if (string.IsNullOrWhiteSpace(body))
        {
            return results;
        }

        var decoded = TryDecodeBase64(body) ?? body;

        var lines = decoded
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (!line.StartsWith("vless://", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (VlessUrlParser.TryParse(line, out var config, out _) && config is not null)
            {
                results.Add(config);
            }
        }

        return results;
    }

    private static string? TryDecodeBase64(string input)
    {
        var cleaned = new string(input
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray())
            .Replace('-', '+')
            .Replace('_', '/');

        switch (cleaned.Length % 4)
        {
            case 2: cleaned += "=="; break;
            case 3: cleaned += "="; break;
            case 1: return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(cleaned);
            var decoded = Encoding.UTF8.GetString(bytes);
            // Heuristic: a valid decoded subscription should contain at least one
            // vless:// or vmess:// or trojan:// scheme.
            if (decoded.Contains("vless://", StringComparison.OrdinalIgnoreCase) ||
                decoded.Contains("vmess://", StringComparison.OrdinalIgnoreCase) ||
                decoded.Contains("trojan://", StringComparison.OrdinalIgnoreCase) ||
                decoded.Contains("ss://", StringComparison.OrdinalIgnoreCase))
            {
                return decoded;
            }
        }
        catch (FormatException)
        {
            // not base64; fall through
        }

        return null;
    }
}
