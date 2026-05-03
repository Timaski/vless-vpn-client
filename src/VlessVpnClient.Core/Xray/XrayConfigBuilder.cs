using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using VlessVpnClient.Core.Models;

namespace VlessVpnClient.Core.Xray;

/// <summary>
/// Builds an xray-core <c>config.json</c> for a given server profile and app settings.
/// </summary>
public static class XrayConfigBuilder
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public static string Build(ServerProfile profile, AppSettings settings)
    {
        var root = new JsonObject
        {
            ["log"] = new JsonObject
            {
                ["loglevel"] = settings.LogLevel
            },
            ["inbounds"] = BuildInbounds(settings),
            ["outbounds"] = BuildOutbounds(profile.Config),
            ["routing"] = BuildRouting(settings)
        };

        return root.ToJsonString(WriteOptions);
    }

    private static JsonArray BuildInbounds(AppSettings settings)
    {
        var inbounds = new JsonArray();

        if (settings.EnableHttpInbound)
        {
            inbounds.Add(new JsonObject
            {
                ["tag"] = "http-in",
                ["port"] = settings.HttpInboundPort,
                ["listen"] = "127.0.0.1",
                ["protocol"] = "http",
                ["sniffing"] = new JsonObject
                {
                    ["enabled"] = true,
                    ["destOverride"] = new JsonArray { "http", "tls" }
                },
                ["settings"] = new JsonObject
                {
                    ["allowTransparent"] = false
                }
            });
        }

        if (settings.EnableSocksInbound)
        {
            inbounds.Add(new JsonObject
            {
                ["tag"] = "socks-in",
                ["port"] = settings.SocksInboundPort,
                ["listen"] = "127.0.0.1",
                ["protocol"] = "socks",
                ["sniffing"] = new JsonObject
                {
                    ["enabled"] = true,
                    ["destOverride"] = new JsonArray { "http", "tls" }
                },
                ["settings"] = new JsonObject
                {
                    ["auth"] = "noauth",
                    ["udp"] = true
                }
            });
        }

        return inbounds;
    }

    private static JsonArray BuildOutbounds(VlessConfig config)
    {
        var streamSettings = BuildStreamSettings(config);

        var vlessOutbound = new JsonObject
        {
            ["tag"] = "proxy",
            ["protocol"] = "vless",
            ["settings"] = new JsonObject
            {
                ["vnext"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["address"] = config.Address,
                        ["port"] = config.Port,
                        ["users"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["id"] = config.Id,
                                ["encryption"] = string.IsNullOrEmpty(config.Encryption) ? "none" : config.Encryption,
                                ["flow"] = config.Flow ?? string.Empty
                            }
                        }
                    }
                }
            },
            ["streamSettings"] = streamSettings
        };

        return new JsonArray
        {
            vlessOutbound,
            new JsonObject
            {
                ["tag"] = "direct",
                ["protocol"] = "freedom",
                ["settings"] = new JsonObject()
            },
            new JsonObject
            {
                ["tag"] = "block",
                ["protocol"] = "blackhole",
                ["settings"] = new JsonObject()
            }
        };
    }

    private static JsonObject BuildStreamSettings(VlessConfig c)
    {
        var stream = new JsonObject
        {
            ["network"] = string.IsNullOrEmpty(c.Network) ? "tcp" : c.Network,
            ["security"] = string.IsNullOrEmpty(c.Security) ? "none" : c.Security
        };

        switch (c.Security.ToLowerInvariant())
        {
            case "tls":
                var tls = new JsonObject();
                if (!string.IsNullOrEmpty(c.Sni)) tls["serverName"] = c.Sni;
                if (!string.IsNullOrEmpty(c.Fingerprint)) tls["fingerprint"] = c.Fingerprint;
                if (!string.IsNullOrEmpty(c.Alpn))
                {
                    var alpn = new JsonArray();
                    foreach (var v in c.Alpn.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        alpn.Add(v);
                    }
                    tls["alpn"] = alpn;
                }
                tls["allowInsecure"] = false;
                stream["tlsSettings"] = tls;
                break;

            case "reality":
                var reality = new JsonObject();
                if (!string.IsNullOrEmpty(c.Sni)) reality["serverName"] = c.Sni;
                if (!string.IsNullOrEmpty(c.Fingerprint)) reality["fingerprint"] = c.Fingerprint;
                if (!string.IsNullOrEmpty(c.PublicKey)) reality["publicKey"] = c.PublicKey;
                if (!string.IsNullOrEmpty(c.ShortId)) reality["shortId"] = c.ShortId;
                reality["spiderX"] = c.SpiderX ?? string.Empty;
                stream["realitySettings"] = reality;
                break;
        }

        switch (c.Network.ToLowerInvariant())
        {
            case "ws":
                var ws = new JsonObject
                {
                    ["path"] = string.IsNullOrEmpty(c.Path) ? "/" : c.Path
                };
                if (!string.IsNullOrEmpty(c.Host))
                {
                    ws["headers"] = new JsonObject { ["Host"] = c.Host };
                }
                stream["wsSettings"] = ws;
                break;

            case "grpc":
                var grpc = new JsonObject
                {
                    ["serviceName"] = c.ServiceName ?? string.Empty
                };
                if (string.Equals(c.Mode, "multi", StringComparison.OrdinalIgnoreCase))
                {
                    grpc["multiMode"] = true;
                }
                stream["grpcSettings"] = grpc;
                break;

            case "http":
            case "h2":
                var http = new JsonObject
                {
                    ["path"] = string.IsNullOrEmpty(c.Path) ? "/" : c.Path
                };
                if (!string.IsNullOrEmpty(c.Host))
                {
                    var hosts = new JsonArray();
                    foreach (var h in c.Host.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        hosts.Add(h);
                    }
                    http["host"] = hosts;
                }
                stream["httpSettings"] = http;
                stream["network"] = "h2";
                break;

            case "kcp":
            case "mkcp":
                var kcp = new JsonObject
                {
                    ["header"] = new JsonObject
                    {
                        ["type"] = string.IsNullOrEmpty(c.HeaderType) ? "none" : c.HeaderType
                    }
                };
                if (!string.IsNullOrEmpty(c.Seed)) kcp["seed"] = c.Seed;
                stream["kcpSettings"] = kcp;
                stream["network"] = "kcp";
                break;

            case "quic":
                var quic = new JsonObject
                {
                    ["security"] = string.IsNullOrEmpty(c.QuicSecurity) ? "none" : c.QuicSecurity,
                    ["key"] = c.Key ?? string.Empty,
                    ["header"] = new JsonObject
                    {
                        ["type"] = string.IsNullOrEmpty(c.HeaderType) ? "none" : c.HeaderType
                    }
                };
                stream["quicSettings"] = quic;
                break;

            case "tcp":
            default:
                if (string.Equals(c.HeaderType, "http", StringComparison.OrdinalIgnoreCase))
                {
                    var tcpHeader = new JsonObject
                    {
                        ["type"] = "http"
                    };
                    if (!string.IsNullOrEmpty(c.Host) || !string.IsNullOrEmpty(c.Path))
                    {
                        var request = new JsonObject
                        {
                            ["path"] = new JsonArray { string.IsNullOrEmpty(c.Path) ? "/" : c.Path }
                        };
                        if (!string.IsNullOrEmpty(c.Host))
                        {
                            var hostsArr = new JsonArray();
                            foreach (var h in c.Host.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                            {
                                hostsArr.Add(h);
                            }
                            request["headers"] = new JsonObject { ["Host"] = hostsArr };
                        }
                        tcpHeader["request"] = request;
                    }
                    stream["tcpSettings"] = new JsonObject { ["header"] = tcpHeader };
                }
                break;
        }

        return stream;
    }

    private static JsonObject BuildRouting(AppSettings settings)
    {
        var rules = new JsonArray();

        if (settings.BypassLan)
        {
            rules.Add(new JsonObject
            {
                ["type"] = "field",
                ["outboundTag"] = "direct",
                ["ip"] = new JsonArray { "geoip:private" }
            });
        }

        rules.Add(new JsonObject
        {
            ["type"] = "field",
            ["outboundTag"] = "block",
            ["protocol"] = new JsonArray { "bittorrent" }
        });

        return new JsonObject
        {
            ["domainStrategy"] = "IPIfNonMatch",
            ["rules"] = rules
        };
    }
}
