using Acme.Admin.Api.Domain;
using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Acme.Admin.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/sales/orders")]
public sealed class OrderController(OrderService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.OrdersRead)]
    public async Task<ActionResult<PagedResult<OrderEntity>>> List(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int size = Paging.DefaultSize,
        CancellationToken ct = default)
    {
        var (p, s) = Paging.Normalize(page, size);
        return Ok(await service.ListAsync(p, s, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.OrdersWrite)]
    public async Task<ActionResult<OrderEntity>> Create([FromBody] OrderCreateRequest req, CancellationToken ct)
    {
        return Ok(await service.CreateAsync(req, ct));
    }
}
