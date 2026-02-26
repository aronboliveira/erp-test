using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Services;
using Acme.Admin.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Admin.Api.Controllers;

[ApiController]
[Route("api/finance/expenses")]
public sealed class ExpenseController(ExpenseService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.FinanceRead)]
    public async Task<ActionResult<PagedResult<ExpenseResponse>>> List(
        [FromQuery] int page = Paging.DefaultPage,
        [FromQuery] int size = Paging.DefaultSize,
        CancellationToken ct = default)
    {
        var (p, s) = Paging.Normalize(page, size);
        return Ok(await service.ListAsync(p, s, ct));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.FinanceWrite)]
    public async Task<ActionResult<ExpenseResponse>> Create([FromBody] ExpenseCreateRequest req, CancellationToken ct)
    {
        return Ok(await service.CreateAsync(req, ct));
    }
}
