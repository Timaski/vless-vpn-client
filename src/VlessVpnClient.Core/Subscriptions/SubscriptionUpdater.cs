using System.Net.Http;
using VlessVpnClient.Core.Logging;
using VlessVpnClient.Core.Models;
using VlessVpnClient.Core.Parsing;

namespace VlessVpnClient.Core.Subscriptions;

public sealed class SubscriptionFetchResult
{
    public required Subscription Subscription { get; init; }
    public IReadOnlyList<VlessConfig> Configs { get; init; } = Array.Empty<VlessConfig>();
    public string? Error { get; init; }
    public bool Success => Error is null;
}

public sealed class SubscriptionUpdater
{
    private readonly IAppLogger _logger;
    private readonly HttpClient _http;
    private readonly string _defaultUserAgent;

    public SubscriptionUpdater(IAppLogger logger, string defaultUserAgent, HttpClient? http = null)
    {
        _logger = logger;
        _defaultUserAgent = defaultUserAgent;
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<SubscriptionFetchResult> FetchAsync(Subscription subscription, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, subscription.Url);
            var ua = subscription.UserAgent ?? _defaultUserAgent;
            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(ua);

            using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            var configs = SubscriptionParser.Parse(body);
            _logger.LogInfo($"Subscription '{subscription.Name}' returned {configs.Count} configs");

            subscription.LastUpdated = DateTime.UtcNow;
            subscription.LastFetchedCount = configs.Count;

            return new SubscriptionFetchResult
            {
                Subscription = subscription,
                Configs = configs
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Subscription '{subscription.Name}' fetch failed: {ex.Message}");
            return new SubscriptionFetchResult
            {
                Subscription = subscription,
                Error = ex.Message
            };
        }
    }
}
