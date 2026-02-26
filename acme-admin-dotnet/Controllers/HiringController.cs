using Acme.Admin.Api.Domain;
using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Acme.Admin.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/hr/hirings")]
public sealed class HiringController(HiringService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.HrRead)]
    public async Task<ActionResult<PagedResult<HiringEntity>>> List(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int size = Paging.DefaultSize,
        CancellationToken ct = default)
    {
        var (p, s) = Paging.Normalize(page, size);
        return Ok(await service.ListAsync(p, s, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.HrWrite)]
    public async Task<ActionResult<HiringEntity>> Create([FromBody] HiringCreateRequest req, CancellationToken ct)
    {
        return Ok(await service.CreateAsync(req, ct));
    }
}
