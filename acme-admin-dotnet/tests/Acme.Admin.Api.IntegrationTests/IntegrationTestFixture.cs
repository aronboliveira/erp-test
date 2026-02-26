using Acme.Admin.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace Acme.Admin.Api.IntegrationTests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("acmedb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public HttpClient Client { get; private set; } = null!;
    private ApiWebApplicationFactory Factory { get; set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var testConnectionString = _postgres.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", testConnectionString);
        Environment.SetEnvironmentVariable("Auth__EnableMockHeader", "true");
        Environment.SetEnvironmentVariable("Stripe__PublishableKey", "pk_test_noop");
        Environment.SetEnvironmentVariable("Billing__Stripe__WebhookSecret", "whsec_dev");
        Environment.SetEnvironmentVariable("Billing__Stripe__SuccessUrl", "http://localhost:14000/s");
        Environment.SetEnvironmentVariable("Billing__Stripe__CancelUrl", "http://localhost:14000/c");

        Factory = new ApiWebApplicationFactory(testConnectionString);
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        Environment.SetEnvironmentVariable("ConnectionStrings__Default", null);
        Environment.SetEnvironmentVariable("Auth__EnableMockHeader", null);
        Environment.SetEnvironmentVariable("Stripe__PublishableKey", null);
        Environment.SetEnvironmentVariable("Billing__Stripe__WebhookSecret", null);
        Environment.SetEnvironmentVariable("Billing__Stripe__SuccessUrl", null);
        Environment.SetEnvironmentVariable("Billing__Stripe__CancelUrl", null);

        await _postgres.DisposeAsync();
    }

    public async Task WithDbContextAsync(Func<AcmeDbContext, Task> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AcmeDbContext>();
        await action(db);
    }

    public WebApplicationFactory<Program> CreateFactoryWithOverrides(
        IReadOnlyDictionary<string, string?> overrides,
        string? environment = null)
    {
        return Factory.WithWebHostBuilder(builder =>
        {
            if (!string.IsNullOrWhiteSpace(environment))
            {
                builder.UseEnvironment(environment);
            }

            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(overrides));
        });
    }

    private static string ResolveApiProjectRoot()
    {
        var cursor = new DirectoryInfo(AppContext.BaseDirectory);
        while (cursor is not null)
        {
            var csproj = Path.Combine(cursor.FullName, "acme-admin-dotnet", "Acme.Admin.Api.csproj");
            if (File.Exists(csproj))
            {
                return Path.Combine(cursor.FullName, "acme-admin-dotnet");
            }

            cursor = cursor.Parent;
        }

        throw new InvalidOperationException("Could not resolve acme-admin-dotnet content root.");
    }

    private sealed class ApiWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override IHostBuilder? CreateHostBuilder()
        {
            var builder = base.CreateHostBuilder();
            builder?.ConfigureWebHost(webBuilder =>
                webBuilder.UseSetting("TEST_CONTENTROOT_ACME_ADMIN_API", ResolveApiProjectRoot()));
            return builder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(ResolveApiProjectRoot());
            builder.UseEnvironment("IntegrationTests");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = connectionString,
                    ["Auth:EnableMockHeader"] = "true",
                    ["SchemaMigration:Enabled"] = "true",
                    ["SchemaMigration:BaselineExistingSchema"] = "true",
                    ["Stripe:PublishableKey"] = "pk_test_noop",
                    ["Billing:Stripe:WebhookSecret"] = "whsec_dev",
                    ["Billing:Stripe:SuccessUrl"] = "http://localhost:14000/s",
                    ["Billing:Stripe:CancelUrl"] = "http://localhost:14000/c"
                });
            });
        }
    }
}

[CollectionDefinition("integration")]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>;
