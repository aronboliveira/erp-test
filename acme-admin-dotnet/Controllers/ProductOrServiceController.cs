using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Acme.Admin.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/catalog/items")]
public sealed class ProductOrServiceController(ProductOrServiceService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.CatalogRead)]
    public async Task<ActionResult<PagedResult<ProductOrServiceResponse>>> List(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int size = Paging.DefaultSize,
        CancellationToken ct = default)
    {
        var (p, s) = Paging.Normalize(page, size);
        return Ok(await service.ListItemsAsync(p, s, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.CatalogWrite)]
    public async Task<ActionResult<ProductOrServiceResponse>> Create([FromBody] ProductOrServiceCreateRequest req, CancellationToken ct)
    {
        return Ok(await service.CreateItemAsync(req, ct));
    }
}
