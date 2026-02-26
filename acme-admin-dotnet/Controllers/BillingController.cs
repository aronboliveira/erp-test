using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/billing")]
public sealed class BillingController(BillingService service, StripePaymentService stripePaymentService) : ControllerBase
{
    [HttpPost("checkout-session")]
    [Authorize(Policy = PermissionPolicies.BillingCreate)]
    public async Task<ActionResult<BillingDtos.CheckoutSessionResponse>> CreateCheckoutSession([FromBody] BillingDtos.CreateCheckoutSessionRequest req, CancellationToken ct)
    {
        var created = await service.CreateCheckoutSessionAsync(req, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> Webhook(
        [FromHeader(Name = "Stripe-Signature")] string? signature,
        [FromBody] JsonElement payload,
        CancellationToken ct)
    {
        return Ok(await service.SaveWebhookAsync(signature, payload.GetRawText(), ct));
    }

    [HttpPost("payment-intents")]
    [Authorize(Policy = PermissionPolicies.BillingPay)]
    public async Task<ActionResult<StripePaymentDtos.CreatePaymentIntentResponse>> CreatePaymentIntent(
        [FromBody] StripePaymentDtos.CreatePaymentIntentRequest req,
        CancellationToken ct)
    {
        return Ok(await stripePaymentService.CreateAsync(req, ct));
    }
}
