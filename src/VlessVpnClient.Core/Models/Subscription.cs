namespace VlessVpnClient.Core.Models;

/// <summary>
/// A user-added subscription URL that resolves to one or more VLESS configs.
/// </summary>
public sealed class Subscription
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public required string Name { get; init; }
    public required string Url { get; init; }
    public DateTime? LastUpdated { get; set; }
    public int LastFetchedCount { get; set; }
    public bool AutoUpdate { get; set; } = true;
    public string? UserAgent { get; set; }
}
