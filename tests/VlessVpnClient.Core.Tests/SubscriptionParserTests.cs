using System.Text;
using VlessVpnClient.Core.Parsing;
using Xunit;

namespace VlessVpnClient.Core.Tests;

public class SubscriptionParserTests
{
    [Fact]
    public void Parses_plaintext_subscription()
    {
        const string body = """
            vless://abc@1.1.1.1:443?security=tls&sni=cf.com#A
            vless://def@2.2.2.2:8443?security=reality&pbk=KEY&sid=12#B
            """;

        var configs = SubscriptionParser.Parse(body);

        Assert.Equal(2, configs.Count);
        Assert.Equal("1.1.1.1", configs[0].Address);
        Assert.Equal("2.2.2.2", configs[1].Address);
    }

    [Fact]
    public void Parses_base64_subscription()
    {
        const string plain = """
            vless://abc@1.1.1.1:443?security=tls&sni=cf.com#A
            vless://def@2.2.2.2:8443?security=reality&pbk=KEY&sid=12#B
            """;
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(plain));

        var configs = SubscriptionParser.Parse(encoded);

        Assert.Equal(2, configs.Count);
    }

    [Fact]
    public void Skips_unknown_protocols()
    {
        const string body = """
            vless://abc@1.1.1.1:443
            vmess://eyJ2IjoiMiJ9
            trojan://pwd@host:443
            """;

        var configs = SubscriptionParser.Parse(body);

        Assert.Single(configs);
        Assert.Equal("1.1.1.1", configs[0].Address);
    }

    [Fact]
    public void Returns_empty_for_empty_body()
    {
        Assert.Empty(SubscriptionParser.Parse(string.Empty));
        Assert.Empty(SubscriptionParser.Parse("   "));
    }
}
