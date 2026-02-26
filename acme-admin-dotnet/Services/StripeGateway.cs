using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Acme.Admin.Api.Configuration;
using Acme.Admin.Api.DTO;
using Microsoft.Extensions.Options;

namespace Acme.Admin.Api.Services;

public sealed class StripeGateway(
    HttpClient http,
    IOptions<BillingStripeOptions> billingOptions,
    IOptions<StripeOptions> stripeOptions,
    ILogger<StripeGateway> logger)
{
    public bool CanCreateCheckoutSession => !string.IsNullOrWhiteSpace(GetSecretKey());

    public bool CanCreatePaymentIntent =>
        !string.IsNullOrWhiteSpace(GetSecretKey()) &&
        !string.IsNullOrWhiteSpace(GetPublishableKey());

    public string GetPublishableKey()
    {
        return stripeOptions.Value.PublishableKey;
    }

    public string GetWebhookSecret()
    {
        return billingOptions.Value.WebhookSecret;
    }

    public async Task<BillingDtos.CheckoutSessionResponse> CreateCheckoutSessionAsync(
        string currency,
        string? customerEmail,
        IReadOnlyList<BillingDtos.LineItem> items,
        string successUrl,
        string cancelUrl,
        CancellationToken ct = default)
    {
        var secretKey = GetSecretKey();
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("stripe.secret-key not configured");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/checkout/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

        var body = new List<KeyValuePair<string, string>>
        {
            new("mode", "payment"),
            new("success_url", successUrl),
            new("cancel_url", cancelUrl)
        };

        if (!string.IsNullOrWhiteSpace(customerEmail))
        {
            body.Add(new("customer_email", customerEmail));
        }

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            body.Add(new($"line_items[{i}][price_data][currency]", currency));
            body.Add(new($"line_items[{i}][price_data][unit_amount]", item.UnitAmountCents.ToString(CultureInfo.InvariantCulture)));
            body.Add(new($"line_items[{i}][price_data][product_data][name]", item.Name ?? string.Empty));
            body.Add(new($"line_items[{i}][quantity]", item.Quantity.ToString(CultureInfo.InvariantCulture)));
        }

        request.Content = new FormUrlEncodedContent(body);

        using var response = await http.SendAsync(request, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"stripe checkout error: {ExtractStripeError(json)}");
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var sessionId = root.TryGetProperty("id", out var idNode) ? idNode.GetString() : null;
        var url = root.TryGetProperty("url", out var urlNode) ? urlNode.GetString() : null;

        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException("stripe checkout response invalid");
        }

        return new BillingDtos.CheckoutSessionResponse("stripe", sessionId, url);
    }

    public async Task<StripePaymentDtos.CreatePaymentIntentResponse> CreatePaymentIntentAsync(
        StripePaymentDtos.CreatePaymentIntentRequest req,
        CancellationToken ct = default)
    {
        var secretKey = GetSecretKey();
        var publishableKey = GetPublishableKey();

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("stripe.secret-key not configured");
        }

        if (string.IsNullOrWhiteSpace(publishableKey))
        {
            throw new InvalidOperationException("stripe.publishable-key not configured");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/payment_intents");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

        var body = new List<KeyValuePair<string, string>>
        {
            new("currency", req.Currency!.Trim().ToLowerInvariant()),
            new("amount", req.AmountCents.ToString(CultureInfo.InvariantCulture)),
            new("automatic_payment_methods[enabled]", "true")
        };

        if (!string.IsNullOrWhiteSpace(req.CustomerEmail))
        {
            body.Add(new("receipt_email", req.CustomerEmail));
        }

        if (!string.IsNullOrWhiteSpace(req.Description))
        {
            body.Add(new("description", req.Description));
        }

        request.Content = new FormUrlEncodedContent(body);

        using var response = await http.SendAsync(request, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"stripe payment-intent error: {ExtractStripeError(json)}");
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var paymentIntentId = root.TryGetProperty("id", out var idNode) ? idNode.GetString() : null;
        var clientSecret = root.TryGetProperty("client_secret", out var secretNode) ? secretNode.GetString() : null;
        var status = root.TryGetProperty("status", out var statusNode) ? statusNode.GetString() : null;

        if (string.IsNullOrWhiteSpace(paymentIntentId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException("stripe payment-intent response invalid");
        }

        return new StripePaymentDtos.CreatePaymentIntentResponse(
            "stripe",
            publishableKey,
            paymentIntentId,
            clientSecret,
            string.IsNullOrWhiteSpace(status) ? "unknown" : status);
    }

    public StripeEventSnapshot VerifyAndReadEvent(string signatureHeader, string payload)
    {
        var webhookSecret = GetWebhookSecret();
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            throw new InvalidOperationException("stripe: webhook secret not configured");
        }

        VerifyStripeSignature(signatureHeader, payload, webhookSecret);

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var eventId = root.TryGetProperty("id", out var idNode) ? idNode.GetString() : null;
        var eventType = root.TryGetProperty("type", out var typeNode) ? typeNode.GetString() : null;

        if (string.IsNullOrWhiteSpace(eventId))
        {
            throw new ArgumentException("stripe: payload missing id");
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("stripe: payload missing type");
        }

        return new StripeEventSnapshot(
            eventId,
            eventType);
    }

    private void VerifyStripeSignature(string signatureHeader, string payload, string secret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            throw new ArgumentException("stripe: signature required");
        }

        var tokens = signatureHeader
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Split('=', 2, StringSplitOptions.TrimEntries))
            .Where(x => x.Length == 2)
            .ToLookup(x => x[0], x => x[1], StringComparer.OrdinalIgnoreCase);

        var timestampRaw = tokens["t"].FirstOrDefault();
        var signatures = tokens["v1"].ToArray();

        if (string.IsNullOrWhiteSpace(timestampRaw) || signatures.Length == 0)
        {
            throw new ArgumentException("stripe: signature invalid");
        }

        if (!long.TryParse(timestampRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp))
        {
            throw new ArgumentException("stripe: signature invalid");
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        const long toleranceSeconds = 300;

        if (Math.Abs(now - timestamp) > toleranceSeconds)
        {
            throw new ArgumentException("stripe: signature timestamp outside tolerance");
        }

        var signedPayload = $"{timestampRaw}.{payload}";
        var hash = ComputeHexHmac(secret, signedPayload);

        var valid = signatures.Any(sig => ConstantTimeEquals(hash, sig));
        if (!valid)
        {
            throw new ArgumentException("stripe: signature verification failed");
        }
    }

    private static string ComputeHexHmac(string secret, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool ConstantTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private string GetSecretKey()
    {
        return billingOptions.Value.SecretKey;
    }

    private string ExtractStripeError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("error", out var errorNode))
            {
                return "unknown error";
            }

            var message = errorNode.TryGetProperty("message", out var msgNode) ? msgNode.GetString() : null;
            var code = errorNode.TryGetProperty("code", out var codeNode) ? codeNode.GetString() : null;

            if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(message))
            {
                return $"{code}: {message}";
            }

            return message ?? "unknown error";
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to parse Stripe error payload");
            return "unknown error";
        }
    }

    public sealed record StripeEventSnapshot(string EventId, string EventType);
}
