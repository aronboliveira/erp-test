using Acme.Admin.Api.Domain;
using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Acme.Admin.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/procurement/purchases")]
public sealed class PurchaseController(PurchaseService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.ProcurementRead)]
    public async Task<ActionResult<PagedResult<PurchaseEntity>>> List(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int size = Paging.DefaultSize,
        CancellationToken ct = default)
    {
        var (p, s) = Paging.Normalize(page, size);
        return Ok(await service.ListAsync(p, s, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.ProcurementWrite)]
    public async Task<ActionResult<PurchaseEntity>> Create([FromBody] PurchaseCreateRequest req, CancellationToken ct)
    {
        return Ok(await service.CreateAsync(req, ct));
    }
}
