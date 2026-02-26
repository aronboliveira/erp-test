using Microsoft.Extensions.Configuration;

namespace Acme.Admin.Api.Configuration;

public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException(
            "Missing required connection string 'ConnectionStrings:Default'.");
    }
}
