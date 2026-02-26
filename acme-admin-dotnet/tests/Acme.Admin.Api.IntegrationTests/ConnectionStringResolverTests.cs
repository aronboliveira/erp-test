using Acme.Admin.Api.Configuration;
using Microsoft.Extensions.Configuration;

namespace Acme.Admin.Api.IntegrationTests;

public sealed class ConnectionStringResolverTests
{
    [Fact]
    public void ResolveThrowsWhenConnectionStringIsMissingOrEmpty()
    {
        var missing = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        Assert.Throws<InvalidOperationException>(() => ConnectionStringResolver.Resolve(missing));

        var empty = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = ""
            })
            .Build();

        Assert.Throws<InvalidOperationException>(() => ConnectionStringResolver.Resolve(empty));
    }

    [Fact]
    public void ResolveReturnsConfiguredConnectionString()
    {
        const string expected = "Host=localhost;Port=5434;Database=acmedb;Username=postgres;Password=postgres";

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = expected
            })
            .Build();

        var actual = ConnectionStringResolver.Resolve(config);
        Assert.Equal(expected, actual);
    }
}
