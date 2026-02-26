using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Acme.Admin.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/billing/events")]
public sealed class BillingEventController(BillingEventService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.BillingRead)]
    public async Task<ActionResult<BillingEventDtos.PageResponse>> Page(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int size = Paging.DefaultSize,
        [FromQuery] string? provider = null,
        [FromQuery] string? eventType = null,
        [FromQuery] string? receivedFrom = null,
        [FromQuery] string? receivedTo = null,
        CancellationToken ct = default)
    {
        var (normalizedPage, normalizedSize) = Paging.Normalize(page, size);

        var from = ParseDatetimeLocalOrNull("receivedFrom", receivedFrom);
        var to = ParseDatetimeLocalOrNull("receivedTo", receivedTo);

        DateValidator.AssertRange(from, to);

        return Ok(await service.PageAsync(normalizedPage, normalizedSize, provider, eventType, from, to, ct));
    }

    private static DateTime? ParseDatetimeLocalOrNull(string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateValidator.IsDatetimeLocal(value))
        {
            throw new ArgumentException($"{field} must match yyyy-MM-dd'T'HH:mm");
        }

        return DateValidator.ParseDatetimeLocal(value);
    }
}
