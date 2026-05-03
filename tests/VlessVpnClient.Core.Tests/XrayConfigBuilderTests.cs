using System.Text.Json;
using VlessVpnClient.Core.Models;
using VlessVpnClient.Core.Parsing;
using VlessVpnClient.Core.Xray;
using Xunit;

namespace VlessVpnClient.Core.Tests;

public class XrayConfigBuilderTests
{
    [Fact]
    public void Builds_reality_config_with_expected_shape()
    {
        const string url = "vless://uuid@1.2.3.4:443?type=tcp&security=reality&pbk=PK&fp=chrome&sni=microsoft.com&sid=ABCD&flow=xtls-rprx-vision#R";
        var config = VlessUrlParser.Parse(url);
        var profile = new ServerProfile { Config = config, OriginalUrl = url };

        var json = XrayConfigBuilder.Build(profile, new AppSettings());
        using var doc = JsonDocument.Parse(json);

        var inbounds = doc.RootElement.GetProperty("inbounds");
        Assert.True(inbounds.GetArrayLength() >= 1);

        var outbounds = doc.RootElement.GetProperty("outbounds");
        var proxy = outbounds[0];
        Assert.Equal("vless", proxy.GetProperty("protocol").GetString());

        var stream = proxy.GetProperty("streamSettings");
        Assert.Equal("reality", stream.GetProperty("security").GetString());
        var reality = stream.GetProperty("realitySettings");
        Assert.Equal("PK", reality.GetProperty("publicKey").GetString());
        Assert.Equal("microsoft.com", reality.GetProperty("serverName").GetString());

        var users = proxy.GetProperty("settings").GetProperty("vnext")[0].GetProperty("users")[0];
        Assert.Equal("uuid", users.GetProperty("id").GetString());
        Assert.Equal("xtls-rprx-vision", users.GetProperty("flow").GetString());
    }

    [Fact]
    public void Builds_ws_tls_stream_settings()
    {
        const string url = "vless://u@h:443?type=ws&security=tls&path=%2Fapi&host=cdn.example&sni=cdn.example";
        var profile = new ServerProfile { Config = VlessUrlParser.Parse(url), OriginalUrl = url };
        var json = XrayConfigBuilder.Build(profile, new AppSettings());
        using var doc = JsonDocument.Parse(json);

        var stream = doc.RootElement.GetProperty("outbounds")[0].GetProperty("streamSettings");
        Assert.Equal("ws", stream.GetProperty("network").GetString());
        Assert.Equal("tls", stream.GetProperty("security").GetString());
        Assert.Equal("/api", stream.GetProperty("wsSettings").GetProperty("path").GetString());
        Assert.Equal("cdn.example", stream.GetProperty("wsSettings").GetProperty("headers").GetProperty("Host").GetString());
        Assert.Equal("cdn.example", stream.GetProperty("tlsSettings").GetProperty("serverName").GetString());
    }

    [Fact]
    public void Settings_control_inbounds()
    {
        var url = "vless://u@h:443";
        var profile = new ServerProfile { Config = VlessUrlParser.Parse(url), OriginalUrl = url };
        var settings = new AppSettings
        {
            EnableHttpInbound = true,
            EnableSocksInbound = false,
            HttpInboundPort = 12345
        };

        var json = XrayConfigBuilder.Build(profile, settings);
        using var doc = JsonDocument.Parse(json);

        var inbounds = doc.RootElement.GetProperty("inbounds");
        Assert.Equal(1, inbounds.GetArrayLength());
        Assert.Equal(12345, inbounds[0].GetProperty("port").GetInt32());
        Assert.Equal("http", inbounds[0].GetProperty("protocol").GetString());
    }
}
