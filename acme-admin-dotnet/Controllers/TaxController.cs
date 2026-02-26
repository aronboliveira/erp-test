using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Acme.Admin.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/taxes")]
public sealed class TaxController(TaxService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.TaxesRead)]
    public async Task<ActionResult<PagedResult<TaxDtos.TaxResponse>>> List(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int size = Paging.DefaultSize,
        CancellationToken ct = default)
    {
        var (p, s) = Paging.Normalize(page, size);
        return Ok(await service.ListAsync(p, s, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.TaxesWrite)]
    public async Task<ActionResult<TaxDtos.TaxResponse>> Create([FromBody] TaxDtos.CreateTaxRequest req, CancellationToken ct)
    {
        var created = await service.CreateAsync(req, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.TaxesWrite)]
    public async Task<ActionResult<TaxDtos.TaxResponse>> Update([FromRoute] Guid id, [FromBody] TaxDtos.UpdateTaxRequest req, CancellationToken ct)
    {
        return Ok(await service.UpdateAsync(id, req, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.TaxesWrite)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
