using System.Text.Json;
using System.Text.Json.Serialization;
using Acme.Admin.Api.Configuration;
using Acme.Admin.Api.Data;
using Acme.Admin.Api.Middleware;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = ConnectionStringResolver.Resolve(builder.Configuration);

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.Configure<BillingStripeOptions>(builder.Configuration.GetSection("Billing:Stripe"));
builder.Services.Configure<SchemaMigrationOptions>(builder.Configuration.GetSection("SchemaMigration"));

builder.Services.AddDbContext<AcmeDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddAuthentication(MockAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>(
        MockAuthenticationHandler.SchemeName,
        _ => { });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    PermissionPolicies.Register(options);
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var first = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .Select(kvp => new
            {
                Field = kvp.Key,
                Message = kvp.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "validation failed"
            })
            .FirstOrDefault();

        var message = first is null ? "validation failed" : $"{first.Field}: {first.Message}";

        return new UnprocessableEntityObjectResult(new
        {
            error = "validation_error",
            message
        });
    };
});

builder.Services.AddHealthChecks().AddDbContextCheck<AcmeDbContext>(name: "postgres");

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<StripeGateway>(client =>
{
    client.BaseAddress = new Uri("https://api.stripe.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<TaxService>();
builder.Services.AddScoped<ProductOrServiceService>();
builder.Services.AddScoped<ExpenseCategoryService>();
builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<RevenueService>();
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<BillService>();
builder.Services.AddScoped<HiringService>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<BillingEventService>();
builder.Services.AddScoped<StripePaymentService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddHostedService<SchemaMigrationHostedService>();
builder.Services.AddHostedService<SecuritySeedHostedService>();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/actuator/health").AllowAnonymous();

app.Run();

public partial class Program;
