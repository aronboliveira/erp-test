using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Acme.Admin.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
public sealed class UserAdminController(UserService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.UsersRead)]
    public async Task<ActionResult<PagedResult<UserDtos.UserDto>>> List(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int size = Paging.DefaultSize,
        CancellationToken ct = default)
    {
        var (p, s) = Paging.Normalize(page, size);
        return Ok(await service.ListAsync(p, s, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.UsersWrite)]
    public async Task<ActionResult<UserDtos.UserDto>> Create([FromBody] UserDtos.CreateUserRequest req, CancellationToken ct)
    {
        return Ok(await service.CreateAsync(req, ct));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.UsersWrite)]
    public async Task<ActionResult<UserDtos.UserDto>> Update([FromRoute] Guid id, [FromBody] UserDtos.UpdateUserRequest req, CancellationToken ct)
    {
        return Ok(await service.UpdateAsync(id, req, ct));
    }
}
