using Acme.Admin.Api.Domain;
using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Acme.Admin.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/finance/budgets")]
public sealed class BudgetController(BudgetService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.FinanceRead)]
    public async Task<ActionResult<PagedResult<BudgetEntity>>> List(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int size = Paging.DefaultSize,
        CancellationToken ct = default)
    {
        var (p, s) = Paging.Normalize(page, size);
        return Ok(await service.ListAsync(p, s, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.FinanceWrite)]
    public async Task<ActionResult<BudgetEntity>> Create([FromBody] BudgetCreateRequest req, CancellationToken ct)
    {
        return Ok(await service.CreateAsync(req, ct));
    }
}
