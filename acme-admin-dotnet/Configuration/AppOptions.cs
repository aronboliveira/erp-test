namespace Acme.Admin.Api.Configuration;

public sealed class AuthOptions
{
    public bool EnableMockHeader { get; set; }
}

public sealed class StripeOptions
{
    public string PublishableKey { get; set; } = string.Empty;
}

public sealed class BillingStripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public sealed class SchemaMigrationOptions
{
    public bool Enabled { get; set; } = true;
    public bool BaselineExistingSchema { get; set; } = true;
}
