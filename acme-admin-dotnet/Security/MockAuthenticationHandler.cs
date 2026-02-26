using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Acme.Admin.Api.Configuration;
using Acme.Admin.Api.Data;
using Acme.Admin.Api.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Acme.Admin.Api.Security;

public sealed class MockAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<AuthOptions> authOptions,
    IHostEnvironment hostEnvironment,
    IServiceProvider serviceProvider) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "MockOrBasic";
    private static readonly ISet<string> CanonicalPermissionCodes = PermissionCatalog.AllCodes();

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Path.StartsWithSegments("/actuator/health", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var basic = await AuthenticateBasicAsync();
        if (basic is not null)
        {
            return basic;
        }

        if (CanUseMockHeaders(authOptions.Value, hostEnvironment) &&
            Request.Headers.TryGetValue("X-Mock-User", out var mockUser) &&
            !string.IsNullOrWhiteSpace(mockUser))
        {
            var perms = ParsePermissions(Request.Headers["X-Mock-Perms"].ToString());
            return AuthenticateResult.Success(BuildTicket(mockUser.ToString().Trim(), null, perms, ["MockUser"]));
        }

        // Strict auth mode: if neither Basic nor allowed mock headers are provided,
        // the request remains unauthenticated and authorization will challenge with 401.
        return AuthenticateResult.NoResult();
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        // Intentionally no WWW-Authenticate header to avoid browser basic-auth popup prompts.
        return Task.CompletedTask;
    }

    private async Task<AuthenticateResult?> AuthenticateBasicAsync()
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var header))
        {
            return null;
        }

        if (!"Basic".Equals(header.Scheme, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(header.Parameter))
        {
            return null;
        }

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter));
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid basic auth token");
        }

        var idx = decoded.IndexOf(':');
        if (idx < 1)
        {
            return AuthenticateResult.Fail("Invalid basic auth format");
        }

        var username = decoded[..idx];
        var password = decoded[(idx + 1)..];

        var user = await LoadByUsernameAsync(username);
        if (user is null)
        {
            return AuthenticateResult.Fail("Invalid credentials");
        }

        if (user.Status != AuthUserStatus.ACTIVE || !user.Enabled)
        {
            return AuthenticateResult.Fail("User inactive");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return AuthenticateResult.Fail("Invalid credentials");
        }

        return AuthenticateResult.Success(BuildTicket(
            user.Username,
            user.Id,
            FlattenPermissions(user),
            user.Roles.Select(x => x.Name)));
    }

    private AuthenticationTicket BuildTicket(string username, Guid? userId, IEnumerable<string> permissions, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username)
        };

        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        foreach (var role in roles.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal))
        {
            claims.Add(new Claim(PermissionPolicies.PermissionClaim, permission));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        return new AuthenticationTicket(principal, SchemeName);
    }

    private async Task<AuthUserEntity?> LoadByUsernameAsync(string username)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AcmeDbContext>();

        return await db.AuthUsers
            .Include(x => x.Roles)
            .ThenInclude(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    private static IEnumerable<string> FlattenPermissions(AuthUserEntity user)
    {
        return user.Roles
            .SelectMany(x => x.Permissions)
            .Select(x => x.Code)
            .Where(x => CanonicalPermissionCodes.Contains(x))
            .Distinct(StringComparer.Ordinal);
    }

    private static IEnumerable<string> ParsePermissions(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => CanonicalPermissionCodes.Contains(x));
    }

    private static bool CanUseMockHeaders(AuthOptions options, IHostEnvironment environment)
    {
        if (!options.EnableMockHeader)
        {
            return false;
        }

        if (environment.IsDevelopment())
        {
            return true;
        }

        return string.Equals(environment.EnvironmentName, "IntegrationTests", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(environment.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase);
    }
}
