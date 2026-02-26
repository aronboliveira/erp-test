namespace Acme.Admin.Api.IntegrationTests;

internal static class HttpTestHelpers
{
    public static HttpRequestMessage JsonRequest(HttpMethod method, string path, object body)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        return request;
    }

    public static HttpRequestMessage MockAuthed(HttpMethod method, string path, string permissions)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-Mock-User", "integration-admin");
        request.Headers.Add("X-Mock-Perms", permissions);
        return request;
    }

    public static HttpRequestMessage MockAuthedJson(HttpMethod method, string path, string permissions, object body)
    {
        var request = JsonRequest(method, path, body);
        request.Headers.Add("X-Mock-User", "integration-admin");
        request.Headers.Add("X-Mock-Perms", permissions);
        return request;
    }

    public static AuthenticationHeaderValue Basic(string username, string password)
    {
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        return new AuthenticationHeaderValue("Basic", token);
    }

    public static string StripeSignature(string payload, string secret)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signedPayload = $"{timestamp}.{payload}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var digest = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var hex = Convert.ToHexString(digest).ToLowerInvariant();

        return $"t={timestamp},v1={hex}";
    }
}
