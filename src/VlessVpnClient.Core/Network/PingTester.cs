using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using VlessVpnClient.Core.Models;

namespace VlessVpnClient.Core.Network;

public sealed class PingResult
{
    public int? LatencyMs { get; init; }
    public string? Error { get; init; }

    public bool Success => LatencyMs.HasValue;
}

/// <summary>
/// Measures latency to a server. Two modes:
///   1. <see cref="TcpPingAsync"/> — direct TCP handshake to the server endpoint.
///   2. <see cref="RealDelayAsync"/> — HTTP request through the local SOCKS proxy
///      after xray is started (matches Happ's "real delay" behaviour).
/// </summary>
public static class PingTester
{
    public static async Task<PingResult> TcpPingAsync(string host, int port, int timeoutMs = 5000, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return new PingResult { Error = "Empty host" };
        }

        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);
            var completed = await Task.WhenAny(connectTask, Task.Delay(timeoutMs, cts.Token)).ConfigureAwait(false);
            if (completed != connectTask || !client.Connected)
            {
                return new PingResult { Error = "TCP timeout" };
            }
            sw.Stop();
            return new PingResult { LatencyMs = (int)sw.ElapsedMilliseconds };
        }
        catch (Exception ex)
        {
            return new PingResult { Error = ex.Message };
        }
    }

    public static async Task<PingResult> RealDelayAsync(
        string proxyHost,
        int proxyPort,
        string testUrl,
        int timeoutMs = 5000,
        CancellationToken ct = default)
    {
        var handler = new HttpClientHandler
        {
            Proxy = new WebProxy($"http://{proxyHost}:{proxyPort}"),
            UseProxy = true
        };

        try
        {
            using var client = new HttpClient(handler, disposeHandler: true)
            {
                Timeout = TimeSpan.FromMilliseconds(timeoutMs)
            };

            var sw = Stopwatch.StartNew();
            using var resp = await client.GetAsync(testUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            sw.Stop();
            if ((int)resp.StatusCode >= 400)
            {
                return new PingResult { Error = $"HTTP {(int)resp.StatusCode}" };
            }
            return new PingResult { LatencyMs = (int)sw.ElapsedMilliseconds };
        }
        catch (TaskCanceledException)
        {
            return new PingResult { Error = "Real-delay timeout" };
        }
        catch (Exception ex)
        {
            return new PingResult { Error = ex.Message };
        }
    }

    /// <summary>
    /// Pings every supplied profile (TCP only) in parallel with bounded concurrency.
    /// Mutates <see cref="ServerProfile.LatencyMs"/> and related fields.
    /// </summary>
    public static async Task PingAllTcpAsync(
        IEnumerable<ServerProfile> profiles,
        int timeoutMs = 5000,
        int concurrency = 16,
        CancellationToken ct = default)
    {
        using var sem = new SemaphoreSlim(concurrency);
        var tasks = profiles.Select(async p =>
        {
            await sem.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var result = await TcpPingAsync(p.Config.Address, p.Config.Port, timeoutMs, ct).ConfigureAwait(false);
                p.LatencyMs = result.LatencyMs;
                p.LatencyError = result.Error;
                p.LatencyMeasuredAt = DateTime.UtcNow;
            }
            finally
            {
                sem.Release();
            }
        });
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
