namespace VlessVpnClient.Core.Models;

/// <summary>
/// A saved server profile shown in the UI. Wraps a <see cref="VlessConfig"/>
/// with bookkeeping fields like measured latency and source subscription.
/// </summary>
public sealed class ServerProfile
{
    public string ProfileId { get; init; } = Guid.NewGuid().ToString("N");

    public required VlessConfig Config { get; init; }
    public required string OriginalUrl { get; init; }

    public string? SubscriptionId { get; init; }

    public int? LatencyMs { get; set; }
    public DateTime? LatencyMeasuredAt { get; set; }
    public string? LatencyError { get; set; }

    public DateTime AddedAt { get; init; } = DateTime.UtcNow;

    public string DisplayName =>
        string.IsNullOrWhiteSpace(Config.Remark)
            ? $"{Config.Address}:{Config.Port}"
            : Config.Remark;
}
