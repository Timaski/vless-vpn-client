namespace VlessVpnClient.Core.Models;

/// <summary>
/// A parsed VLESS server descriptor. Mirrors the URI scheme used by xray-core.
/// </summary>
public sealed class VlessConfig
{
    public required string Id { get; init; }
    public required string Address { get; init; }
    public required int Port { get; init; }

    public string Encryption { get; init; } = "none";
    public string Flow { get; init; } = string.Empty;

    public string Network { get; init; } = "tcp";
    public string Security { get; init; } = "none";

    public string Sni { get; init; } = string.Empty;
    public string Alpn { get; init; } = string.Empty;
    public string Fingerprint { get; init; } = string.Empty;

    public string PublicKey { get; init; } = string.Empty;
    public string ShortId { get; init; } = string.Empty;
    public string SpiderX { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
    public string ServiceName { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public string HeaderType { get; init; } = string.Empty;
    public string Seed { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string QuicSecurity { get; init; } = string.Empty;

    public string Remark { get; init; } = string.Empty;
}
