using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/me")]
public sealed class MeController(UserService service) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<UserDtos.ProfileDto>> Profile(CancellationToken ct)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized();
        }

        return Ok(await service.ProfileByUsernameAsync(username, ct));
    }
}
