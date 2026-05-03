using VlessVpnClient.Core.Parsing;
using Xunit;

namespace VlessVpnClient.Core.Tests;

public class VlessUrlParserTests
{
    [Fact]
    public void Parses_minimal_vless_url()
    {
        const string url = "vless://uuid-1234@example.com:443";
        var ok = VlessUrlParser.TryParse(url, out var config, out var error);

        Assert.True(ok, error);
        Assert.NotNull(config);
        Assert.Equal("uuid-1234", config!.Id);
        Assert.Equal("example.com", config.Address);
        Assert.Equal(443, config.Port);
        Assert.Equal("none", config.Encryption);
        Assert.Equal("tcp", config.Network);
        Assert.Equal("none", config.Security);
    }

    [Fact]
    public void Parses_reality_vless_url_with_remark()
    {
        const string url = "vless://abcd@1.2.3.4:443?type=tcp&security=reality&pbk=PUBKEY&fp=chrome&sni=microsoft.com&sid=abcd&flow=xtls-rprx-vision#My%20Server";

        var config = VlessUrlParser.Parse(url);

        Assert.Equal("abcd", config.Id);
        Assert.Equal("1.2.3.4", config.Address);
        Assert.Equal(443, config.Port);
        Assert.Equal("reality", config.Security);
        Assert.Equal("PUBKEY", config.PublicKey);
        Assert.Equal("chrome", config.Fingerprint);
        Assert.Equal("microsoft.com", config.Sni);
        Assert.Equal("abcd", config.ShortId);
        Assert.Equal("xtls-rprx-vision", config.Flow);
        Assert.Equal("My Server", config.Remark);
    }

    [Fact]
    public void Parses_ws_tls_url()
    {
        const string url = "vless://uuid@host.example:8443?type=ws&security=tls&path=%2Fws&host=cdn.example&sni=cdn.example#WS";

        var c = VlessUrlParser.Parse(url);

        Assert.Equal("ws", c.Network);
        Assert.Equal("tls", c.Security);
        Assert.Equal("/ws", c.Path);
        Assert.Equal("cdn.example", c.Host);
        Assert.Equal("cdn.example", c.Sni);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("https://example.com")]
    [InlineData("vless://")]
    [InlineData("")]
    public void Rejects_invalid_urls(string url)
    {
        var ok = VlessUrlParser.TryParse(url, out var config, out _);
        Assert.False(ok);
        Assert.Null(config);
    }
}
