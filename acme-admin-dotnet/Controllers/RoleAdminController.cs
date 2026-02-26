using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Acme.Admin.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/roles")]
public sealed class RoleAdminController(RoleService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.RolesRead)]
    public async Task<ActionResult<PagedResult<RoleDtos.RoleDto>>> List(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int size = Paging.DefaultSize,
        CancellationToken ct = default)
    {
        var (p, s) = Paging.Normalize(page, size);
        return Ok(await service.ListAsync(p, s, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.RolesWrite)]
    public async Task<ActionResult<RoleDtos.RoleDto>> Create([FromBody] RoleDtos.CreateRoleRequest req, CancellationToken ct)
    {
        return Ok(await service.CreateAsync(req, ct));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.RolesWrite)]
    public async Task<ActionResult<RoleDtos.RoleDto>> Update([FromRoute] Guid id, [FromBody] RoleDtos.UpdateRoleRequest req, CancellationToken ct)
    {
        return Ok(await service.UpdateAsync(id, req, ct));
    }
}
