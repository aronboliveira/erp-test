using System.Globalization;
using System.Text.RegularExpressions;
using Acme.Admin.Api.Configuration;
using Acme.Admin.Api.Data;
using Acme.Admin.Api.Domain;
using Acme.Admin.Api.DTO;
using Acme.Admin.Api.Security;
using Acme.Admin.Api.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Acme.Admin.Api.Services;

public sealed partial class TaxService(AcmeDbContext db)
{
    public async Task<PagedResult<TaxDtos.TaxResponse>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.Taxes
            .AsNoTracking()
            .OrderBy(x => x.Code);

        var total = await query.LongCountAsync(ct);

        var taxes = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<TaxDtos.TaxResponse>(
            taxes.Select(ToDto).ToList(),
            page,
            size,
            total);
    }

    public async Task<TaxDtos.TaxResponse> CreateAsync(TaxDtos.CreateTaxRequest req, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(req);

        var code = NormalizeCode(req.Code);
        if (await db.Taxes.AnyAsync(x => x.Code == code, ct))
        {
            throw new ArgumentException("tax: code already exists");
        }

        var entity = new TaxEntity
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = NormalizeTitle(req.Title),
            Rate = req.Rate ?? 0m,
            Enabled = true
        };

        db.Taxes.Add(entity);
        await db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<TaxDtos.TaxResponse> UpdateAsync(Guid id, TaxDtos.UpdateTaxRequest req, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("tax: id required");
        }

        ArgumentNullException.ThrowIfNull(req);

        var entity = await db.Taxes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ArgumentException("tax: not found");

        var code = NormalizeCode(req.Code);
        if (!string.Equals(entity.Code, code, StringComparison.Ordinal) && await db.Taxes.AnyAsync(x => x.Code == code, ct))
        {
            throw new ArgumentException("tax: code already exists");
        }

        entity.Code = code;
        entity.Name = NormalizeTitle(req.Title);
        entity.Rate = req.Rate ?? 0m;

        await db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("tax: id required");
        }

        var entity = await db.Taxes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ArgumentException("tax: not found");

        db.Taxes.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    private static TaxDtos.TaxResponse ToDto(TaxEntity entity)
    {
        return new TaxDtos.TaxResponse(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Rate,
            TemporalFormat.DatetimeLocal(entity.CreatedAt));
    }

    private static string NormalizeCode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("tax: code required");
        }

        var value = raw.Trim().ToUpperInvariant();
        if (!CodeRegex().IsMatch(value))
        {
            throw new ArgumentException("tax: code format invalid");
        }

        return value;
    }

    private static string NormalizeTitle(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("tax: title required");
        }

        var value = raw.Trim();
        if (value.Length > 160)
        {
            throw new ArgumentException("tax: title too long");
        }

        return value;
    }

    [GeneratedRegex("^[A-Z0-9_\\-]{2,64}$")]
    private static partial Regex CodeRegex();
}

public sealed class ProductOrServiceService(AcmeDbContext db)
{
    public async Task<PagedResult<ProductOrServiceResponse>> ListItemsAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.ProductsOrServices
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id);

        var total = await query.LongCountAsync(ct);

        var rows = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<ProductOrServiceResponse>(
            rows.Select(ToResponse).ToList(),
            page,
            size,
            total);
    }

    public async Task<ProductOrServiceResponse> CreateItemAsync(ProductOrServiceCreateRequest req, CancellationToken ct = default)
    {
        var categoryId = req.CategoryId ?? throw new ArgumentException("categoryId required");
        var category = await db.ProductOrServiceCategories.FirstOrDefaultAsync(x => x.Id == categoryId, ct)
            ?? throw new ArgumentException($"Category not found: {categoryId}");

        var entity = new ProductOrServiceEntity
        {
            Id = Guid.NewGuid(),
            Kind = req.Kind ?? throw new ArgumentException("kind required"),
            Name = req.Name,
            Sku = string.IsNullOrWhiteSpace(req.Sku) ? null : req.Sku.Trim(),
            Price = req.Price ?? 0m,
            Currency = req.Currency.ToUpperInvariant(),
            CategoryId = category.Id
        };

        db.ProductsOrServices.Add(entity);
        await db.SaveChangesAsync(ct);

        return ToResponse(entity);
    }

    public async Task<PagedResult<ProductOrServiceCategoryEntity>> ListCategoriesAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.ProductOrServiceCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id);

        var total = await query.LongCountAsync(ct);

        var rows = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<ProductOrServiceCategoryEntity>(rows, page, size, total);
    }

    public async Task<ProductOrServiceCategoryEntity> CreateCategoryAsync(ProductOrServiceCategoryCreateRequest req, CancellationToken ct = default)
    {
        var entity = new ProductOrServiceCategoryEntity
        {
            Id = Guid.NewGuid(),
            Code = req.Code.Trim(),
            Name = req.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim()
        };

        db.ProductOrServiceCategories.Add(entity);
        await db.SaveChangesAsync(ct);

        return entity;
    }

    private static ProductOrServiceResponse ToResponse(ProductOrServiceEntity entity)
    {
        return new ProductOrServiceResponse(
            entity.Id,
            entity.Kind,
            entity.Name,
            entity.Sku,
            entity.Price.ToString("0.00", CultureInfo.InvariantCulture),
            entity.Currency,
            entity.CategoryId);
    }
}

public sealed class ExpenseCategoryService(AcmeDbContext db)
{
    public async Task<PagedResult<ExpenseCategoryEntity>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.ExpenseCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id);

        var total = await query.LongCountAsync(ct);
        var rows = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<ExpenseCategoryEntity>(rows, page, size, total);
    }

    public async Task<ExpenseCategoryEntity> CreateAsync(ExpenseCategoryCreateRequest req, CancellationToken ct = default)
    {
        var entity = new ExpenseCategoryEntity
        {
            Id = Guid.NewGuid(),
            Code = req.Code.Trim(),
            Name = req.Name.Trim(),
            Subject = req.Subject,
            Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim()
        };

        db.ExpenseCategories.Add(entity);
        await db.SaveChangesAsync(ct);

        return entity;
    }
}

public sealed class ExpenseService(AcmeDbContext db)
{
    private static readonly OccurredAtPolicy OccurredAtPolicy =
        new(false, TimeSpan.FromDays(3650), TimeSpan.FromMinutes(5));

    public async Task<PagedResult<ExpenseResponse>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.Expenses
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id);

        var total = await query.LongCountAsync(ct);
        var rows = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<ExpenseResponse>(
            rows.Select(ToResponse).ToList(),
            page,
            size,
            total);
    }

    public async Task<ExpenseResponse> CreateAsync(ExpenseCreateRequest req, CancellationToken ct = default)
    {
        var categoryId = req.CategoryId ?? throw new ArgumentException("categoryId required");
        var category = await db.ExpenseCategories.FirstOrDefaultAsync(x => x.Id == categoryId, ct)
            ?? throw new ApiValidationException(
            [
                new ValidationIssue(
                    "categoryId",
                    "Expense category not found",
                    Severity.ERROR,
                    new Dictionary<string, object> { ["categoryId"] = categoryId.ToString() })
            ]);

        var entity = new ExpenseEntity
        {
            Id = Guid.NewGuid(),
            OccurredAt = DomainValidators.EnsureUtc(req.OccurredAt ?? default),
            Amount = req.Amount ?? 0m,
            Currency = req.Currency.ToUpperInvariant(),
            Vendor = req.Vendor,
            CategoryId = category.Id
        };

        var validation = new ValidationResult();
        if (string.IsNullOrWhiteSpace(entity.Currency) || entity.Currency.Length != 3)
        {
            validation.Add(new ValidationIssue("currency", "currency must be ISO-4217 (3 letters)", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (entity.Amount < 0)
        {
            validation.Add(new ValidationIssue("amount", "amount must be >= 0", Severity.ERROR, new Dictionary<string, object>()));
        }

        DomainValidators.ValidateOccurredAt(validation, "occurredAt", entity.OccurredAt, DateTime.UtcNow, OccurredAtPolicy);

        if (validation.HasErrors())
        {
            throw new ApiValidationException(validation.ToList());
        }

        db.Expenses.Add(entity);
        await db.SaveChangesAsync(ct);

        return ToResponse(entity);
    }

    private static ExpenseResponse ToResponse(ExpenseEntity entity)
    {
        return new ExpenseResponse(
            entity.Id,
            entity.OccurredAt.ToString("O", CultureInfo.InvariantCulture),
            entity.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            entity.Currency,
            entity.CategoryId,
            entity.Vendor);
    }
}

public sealed class RevenueService(AcmeDbContext db)
{
    public async Task<PagedResult<RevenueEntity>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.Revenues
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id);

        var total = await query.LongCountAsync(ct);
        var rows = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<RevenueEntity>(rows, page, size, total);
    }

    public async Task<RevenueEntity> CreateAsync(RevenueCreateRequest req, CancellationToken ct = default)
    {
        var entity = new RevenueEntity
        {
            Id = Guid.NewGuid(),
            OccurredAt = DomainValidators.EnsureUtc(req.OccurredAt ?? default),
            Amount = req.Amount ?? 0m,
            Currency = req.Currency.ToUpperInvariant(),
            SourceRef = req.SourceRef
        };

        db.Revenues.Add(entity);
        await db.SaveChangesAsync(ct);

        return entity;
    }
}

public sealed class BudgetService(AcmeDbContext db)
{
    public async Task<PagedResult<BudgetEntity>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.Budgets
            .AsNoTracking()
            .OrderByDescending(x => x.PeriodStart)
            .ThenByDescending(x => x.Id);

        var total = await query.LongCountAsync(ct);
        var rows = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<BudgetEntity>(rows, page, size, total);
    }

    public async Task<BudgetEntity> CreateAsync(BudgetCreateRequest req, CancellationToken ct = default)
    {
        var entity = new BudgetEntity
        {
            Id = Guid.NewGuid(),
            PeriodStart = req.PeriodStart ?? default,
            PeriodEnd = req.PeriodEnd ?? default,
            PlannedAmount = req.PlannedAmount ?? 0m,
            Currency = req.Currency.ToUpperInvariant()
        };

        db.Budgets.Add(entity);
        await db.SaveChangesAsync(ct);

        return entity;
    }
}

public sealed class OrderService(AcmeDbContext db)
{
    private static readonly OccurredAtPolicy OccurredAtPolicy =
        new(true, TimeSpan.FromDays(3650), TimeSpan.FromMinutes(5));

    private static readonly TaxIdsPolicy TaxIdsPolicy =
        new(64, true);

    public async Task<PagedResult<OrderEntity>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id);

        var total = await query.LongCountAsync(ct);
        var rows = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<OrderEntity>(rows, page, size, total);
    }

    public async Task<OrderEntity> CreateAsync(OrderCreateRequest req, CancellationToken ct = default)
    {
        var entity = new OrderEntity
        {
            Id = Guid.NewGuid(),
            Code = req.Code,
            OccurredAt = DomainValidators.EnsureUtc(req.OccurredAt ?? default),
            IssuedAt = DateTimeOffset.UtcNow,
            Currency = req.Currency.ToUpperInvariant(),
            Total = req.Total ?? 0m,
            TaxIds = DomainValidators.NormalizeTaxIds(req.TaxIds)
        };

        var validation = new ValidationResult();
        ValidateTaxLinkedCommon(validation, entity.Code, entity.Currency, entity.Total, entity.OccurredAt);
        validation.AddRange(DomainValidators.ValidateTaxIdsShape(entity.TaxIds?.Select(x => (Guid?)x), TaxIdsPolicy).ToList());

        await ValidateTaxIdsExistenceAsync(validation, entity.TaxIds, ct);

        if (validation.HasErrors())
        {
            throw new ApiValidationException(validation.ToList());
        }

        db.Orders.Add(entity);
        await db.SaveChangesAsync(ct);

        return entity;
    }

    private static void ValidateTaxLinkedCommon(ValidationResult validation, string code, string currency, decimal total, DateTime occurredAt)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            validation.Add(new ValidationIssue("code", "code is required", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            validation.Add(new ValidationIssue("currency", "currency must be ISO-4217 (3 letters)", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (total < 0)
        {
            validation.Add(new ValidationIssue("total", "total must be >= 0", Severity.ERROR, new Dictionary<string, object>()));
        }

        DomainValidators.ValidateOccurredAt(validation, "occurredAt", occurredAt, DateTime.UtcNow, OccurredAtPolicy);
    }

    private async Task ValidateTaxIdsExistenceAsync(ValidationResult validation, List<Guid>? taxIds, CancellationToken ct)
    {
        if (taxIds is null || taxIds.Count == 0)
        {
            return;
        }

        var existing = await db.Taxes.CountAsync(x => taxIds.Contains(x.Id), ct);
        if (existing == taxIds.Count)
        {
            return;
        }

        var known = await db.Taxes
            .Where(x => taxIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        foreach (var taxId in taxIds.Where(x => !known.Contains(x)))
        {
            validation.Add(new ValidationIssue(
                "taxIds",
                "Unknown id",
                Severity.ERROR,
                new Dictionary<string, object> { ["id"] = taxId.ToString() }));
        }
    }
}

public sealed class PurchaseService(AcmeDbContext db)
{
    private static readonly OccurredAtPolicy OccurredAtPolicy =
        new(true, TimeSpan.FromDays(3650), TimeSpan.FromMinutes(5));

    private static readonly TaxIdsPolicy TaxIdsPolicy =
        new(64, true);

    public async Task<PagedResult<PurchaseEntity>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.Purchases
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id);

        var total = await query.LongCountAsync(ct);
        var rows = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<PurchaseEntity>(rows, page, size, total);
    }

    public async Task<PurchaseEntity> CreateAsync(PurchaseCreateRequest req, CancellationToken ct = default)
    {
        var entity = new PurchaseEntity
        {
            Id = Guid.NewGuid(),
            Code = req.Code,
            OccurredAt = DomainValidators.EnsureUtc(req.OccurredAt ?? default),
            Currency = req.Currency.ToUpperInvariant(),
            Total = req.Total ?? 0m,
            Vendor = req.Vendor,
            TaxIds = DomainValidators.NormalizeTaxIds(req.TaxIds)
        };

        var validation = new ValidationResult();
        if (string.IsNullOrWhiteSpace(entity.Code))
        {
            validation.Add(new ValidationIssue("code", "code is required", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (string.IsNullOrWhiteSpace(entity.Currency) || entity.Currency.Length != 3)
        {
            validation.Add(new ValidationIssue("currency", "currency must be ISO-4217 (3 letters)", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (entity.Total < 0)
        {
            validation.Add(new ValidationIssue("total", "total must be >= 0", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (!string.IsNullOrWhiteSpace(entity.Vendor) && entity.Vendor.Length > 180)
        {
            validation.Add(new ValidationIssue("vendor", "vendor must be <= 180 chars", Severity.ERROR, new Dictionary<string, object> { ["maxLen"] = 180 }));
        }

        DomainValidators.ValidateOccurredAt(validation, "occurredAt", entity.OccurredAt, DateTime.UtcNow, OccurredAtPolicy);
        validation.AddRange(DomainValidators.ValidateTaxIdsShape(entity.TaxIds?.Select(x => (Guid?)x), TaxIdsPolicy).ToList());
        await ValidateTaxIdsExistenceAsync(validation, entity.TaxIds, ct);

        if (validation.HasErrors())
        {
            throw new ApiValidationException(validation.ToList());
        }

        db.Purchases.Add(entity);
        await db.SaveChangesAsync(ct);

        return entity;
    }

    private async Task ValidateTaxIdsExistenceAsync(ValidationResult validation, List<Guid>? taxIds, CancellationToken ct)
    {
        if (taxIds is null || taxIds.Count == 0)
        {
            return;
        }

        var existingIds = await db.Taxes
            .Where(x => taxIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        foreach (var taxId in taxIds.Where(x => !existingIds.Contains(x)))
        {
            validation.Add(new ValidationIssue(
                "taxIds",
                "Unknown id",
                Severity.ERROR,
                new Dictionary<string, object> { ["id"] = taxId.ToString() }));
        }
    }
}

public sealed class BillService(AcmeDbContext db)
{
    private static readonly OccurredAtPolicy OccurredAtPolicy =
        new(true, TimeSpan.FromDays(3650), TimeSpan.FromMinutes(5));

    private static readonly TaxIdsPolicy TaxIdsPolicy =
        new(64, true);

    public async Task<PagedResult<BillEntity>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.Bills
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id);

        var total = await query.LongCountAsync(ct);
        var rows = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<BillEntity>(rows, page, size, total);
    }

    public async Task<BillEntity> CreateAsync(BillCreateRequest req, CancellationToken ct = default)
    {
        var entity = new BillEntity
        {
            Id = Guid.NewGuid(),
            Code = req.Code ?? string.Empty,
            OccurredAt = req.OccurredAt.HasValue ? DomainValidators.EnsureUtc(req.OccurredAt.Value) : default,
            DueAt = req.DueAt.HasValue ? DomainValidators.EnsureUtc(req.DueAt.Value) : null,
            Currency = (req.Currency ?? string.Empty).ToUpperInvariant(),
            Total = req.Total ?? -1m,
            Vendor = req.Vendor,
            Payee = req.Payee,
            TaxIds = DomainValidators.NormalizeTaxIds(req.TaxIds)
        };

        var validation = new ValidationResult();
        if (string.IsNullOrWhiteSpace(entity.Code))
        {
            validation.Add(new ValidationIssue("code", "code is required", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (string.IsNullOrWhiteSpace(entity.Currency) || entity.Currency.Length != 3)
        {
            validation.Add(new ValidationIssue("currency", "currency must be ISO-4217 (3 letters)", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (entity.Total < 0)
        {
            validation.Add(new ValidationIssue("total", "total must be >= 0", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (!string.IsNullOrWhiteSpace(entity.Vendor) && entity.Vendor.Length > 180)
        {
            validation.Add(new ValidationIssue("vendor", "vendor must be <= 180 chars", Severity.ERROR, new Dictionary<string, object> { ["maxLen"] = 180 }));
        }

        DomainValidators.ValidateOccurredAt(
            validation,
            "occurredAt",
            req.OccurredAt,
            DateTime.UtcNow,
            OccurredAtPolicy);

        if (entity.DueAt.HasValue && req.OccurredAt.HasValue && entity.DueAt.Value < DomainValidators.EnsureUtc(req.OccurredAt.Value))
        {
            validation.Add(new ValidationIssue(
                "dueAt",
                "dueAt cannot be before occurredAt",
                Severity.ERROR,
                new Dictionary<string, object>
                {
                    ["dueAt"] = entity.DueAt.Value.ToString("O", CultureInfo.InvariantCulture),
                    ["occurredAt"] = DomainValidators.EnsureUtc(req.OccurredAt.Value).ToString("O", CultureInfo.InvariantCulture)
                }));
        }

        validation.AddRange(DomainValidators.ValidateTaxIdsShape(entity.TaxIds?.Select(x => (Guid?)x), TaxIdsPolicy).ToList());
        await ValidateTaxIdsExistenceAsync(validation, entity.TaxIds, ct);

        if (validation.HasErrors())
        {
            throw new ApiValidationException(validation.ToList());
        }

        db.Bills.Add(entity);
        await db.SaveChangesAsync(ct);

        return entity;
    }

    private async Task ValidateTaxIdsExistenceAsync(ValidationResult validation, List<Guid>? taxIds, CancellationToken ct)
    {
        if (taxIds is null || taxIds.Count == 0)
        {
            return;
        }

        var existingIds = await db.Taxes
            .Where(x => taxIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        foreach (var taxId in taxIds.Where(x => !existingIds.Contains(x)))
        {
            validation.Add(new ValidationIssue(
                "taxIds",
                "Unknown id",
                Severity.ERROR,
                new Dictionary<string, object> { ["id"] = taxId.ToString() }));
        }
    }
}

public sealed class HiringService(AcmeDbContext db)
{
    private static readonly OccurredAtPolicy OccurredAtPolicy =
        new(true, TimeSpan.FromDays(3650), TimeSpan.FromMinutes(5));

    private static readonly TaxIdsPolicy TaxIdsPolicy =
        new(64, true);

    public async Task<PagedResult<HiringEntity>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var query = db.Hirings
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id);

        var total = await query.LongCountAsync(ct);
        var rows = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<HiringEntity>(rows, page, size, total);
    }

    public async Task<HiringEntity> CreateAsync(HiringCreateRequest req, CancellationToken ct = default)
    {
        var entity = new HiringEntity
        {
            Id = Guid.NewGuid(),
            Code = req.Code ?? string.Empty,
            OccurredAt = req.OccurredAt.HasValue ? DomainValidators.EnsureUtc(req.OccurredAt.Value) : default,
            EmployeeName = req.EmployeeName ?? string.Empty,
            Role = req.Role ?? string.Empty,
            StartAt = req.StartAt.HasValue ? DomainValidators.EnsureUtc(req.StartAt.Value) : default,
            EndAt = req.EndAt.HasValue ? DomainValidators.EnsureUtc(req.EndAt.Value) : null,
            GrossSalary = req.GrossSalary ?? -1m,
            Currency = (req.Currency ?? string.Empty).ToUpperInvariant(),
            Total = req.Total,
            CandidateName = req.CandidateName,
            TaxIds = DomainValidators.NormalizeTaxIds(req.TaxIds)
        };

        var validation = new ValidationResult();

        if (string.IsNullOrWhiteSpace(entity.Code))
        {
            validation.Add(new ValidationIssue("code", "code is required", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (string.IsNullOrWhiteSpace(entity.EmployeeName))
        {
            validation.Add(new ValidationIssue("employeeName", "employeeName is required", Severity.ERROR, new Dictionary<string, object>()));
        }
        else if (entity.EmployeeName.Length > 180)
        {
            validation.Add(new ValidationIssue("employeeName", "employeeName must be <= 180 chars", Severity.ERROR, new Dictionary<string, object> { ["maxLen"] = 180 }));
        }

        if (string.IsNullOrWhiteSpace(entity.Role))
        {
            validation.Add(new ValidationIssue("role", "role is required", Severity.ERROR, new Dictionary<string, object>()));
        }
        else if (entity.Role.Length > 120)
        {
            validation.Add(new ValidationIssue("role", "role must be <= 120 chars", Severity.ERROR, new Dictionary<string, object> { ["maxLen"] = 120 }));
        }

        if (string.IsNullOrWhiteSpace(entity.Currency) || entity.Currency.Length != 3)
        {
            validation.Add(new ValidationIssue("currency", "currency must be ISO-4217 (3 letters)", Severity.ERROR, new Dictionary<string, object>()));
        }

        if (entity.GrossSalary < 0)
        {
            validation.Add(new ValidationIssue("grossSalary", "grossSalary must be >= 0", Severity.ERROR, new Dictionary<string, object>()));
        }

        DomainValidators.ValidateOccurredAt(validation, "occurredAt", req.OccurredAt, DateTime.UtcNow, OccurredAtPolicy);

        if (!req.StartAt.HasValue)
        {
            validation.Add(new ValidationIssue("startAt", "startAt is required", Severity.ERROR, new Dictionary<string, object>()));
        }
        else if (req.OccurredAt.HasValue && DomainValidators.EnsureUtc(req.StartAt.Value) < DomainValidators.EnsureUtc(req.OccurredAt.Value))
        {
            validation.Add(new ValidationIssue(
                "startAt",
                "startAt cannot be before occurredAt",
                Severity.ERROR,
                new Dictionary<string, object>()));
        }

        if (req.EndAt.HasValue && req.StartAt.HasValue && DomainValidators.EnsureUtc(req.EndAt.Value) < DomainValidators.EnsureUtc(req.StartAt.Value))
        {
            validation.Add(new ValidationIssue(
                "endAt",
                "endAt cannot be before startAt",
                Severity.ERROR,
                new Dictionary<string, object>()));
        }

        validation.AddRange(DomainValidators.ValidateTaxIdsShape(entity.TaxIds?.Select(x => (Guid?)x), TaxIdsPolicy).ToList());
        await ValidateTaxIdsExistenceAsync(validation, entity.TaxIds, ct);

        if (validation.HasErrors())
        {
            throw new ApiValidationException(validation.ToList());
        }

        db.Hirings.Add(entity);
        await db.SaveChangesAsync(ct);

        return entity;
    }

    private async Task ValidateTaxIdsExistenceAsync(ValidationResult validation, List<Guid>? taxIds, CancellationToken ct)
    {
        if (taxIds is null || taxIds.Count == 0)
        {
            return;
        }

        var existingIds = await db.Taxes
            .Where(x => taxIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        foreach (var taxId in taxIds.Where(x => !existingIds.Contains(x)))
        {
            validation.Add(new ValidationIssue(
                "taxIds",
                "Unknown id",
                Severity.ERROR,
                new Dictionary<string, object> { ["id"] = taxId.ToString() }));
        }
    }
}

public sealed class BillingService(AcmeDbContext db, IOptions<BillingStripeOptions> billingOptions, StripeGateway stripeGateway)
{

    public async Task<BillingDtos.CheckoutSessionResponse> CreateCheckoutSessionAsync(BillingDtos.CreateCheckoutSessionRequest req, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(req);

        if (req.Items is null || req.Items.Count == 0)
        {
            throw new ArgumentException("billing: items required");
        }

        var currency = NormalizeCurrency(req.Currency);
        var items = NormalizeItems(req.Items);

        var successUrl = string.IsNullOrWhiteSpace(req.SuccessUrl)
            ? billingOptions.Value.SuccessUrl
            : req.SuccessUrl;

        var cancelUrl = string.IsNullOrWhiteSpace(req.CancelUrl)
            ? billingOptions.Value.CancelUrl
            : req.CancelUrl;

        if (string.IsNullOrWhiteSpace(successUrl))
        {
            throw new ArgumentException("billing: successUrl required");
        }

        if (string.IsNullOrWhiteSpace(cancelUrl))
        {
            throw new ArgumentException("billing: cancelUrl required");
        }

        if (stripeGateway.CanCreateCheckoutSession)
        {
            return await stripeGateway.CreateCheckoutSessionAsync(
                currency,
                req.CustomerEmail,
                items,
                successUrl,
                cancelUrl,
                ct);
        }

        var id = $"cs_noop_{Guid.NewGuid():N}";
        var url = $"{successUrl}?session_id={id}&provider=stripe-noop&currency={currency}";
        return new BillingDtos.CheckoutSessionResponse("stripe", id, url);
    }

    public async Task<string> SaveWebhookAsync(string? signature, string payload, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new ArgumentException("stripe: signature required");
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("stripe: payload required");
        }

        var ev = stripeGateway.VerifyAndReadEvent(signature, payload);

        var exists = await db.BillingEvents.AnyAsync(x => x.EventId == ev.EventId, ct);
        if (exists)
        {
            return "ok";
        }

        db.BillingEvents.Add(new BillingEventEntity
        {
            Id = Guid.NewGuid(),
            Provider = "stripe",
            EventId = ev.EventId,
            EventType = ev.EventType,
            Payload = payload,
            ReceivedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);

        return "ok";
    }

    private static string NormalizeCurrency(string? raw)
    {
        var value = string.IsNullOrWhiteSpace(raw) ? "brl" : raw.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(value, "^[a-z]{3}$"))
        {
            throw new ArgumentException("billing: currency invalid");
        }

        return value;
    }

    private static List<BillingDtos.LineItem> NormalizeItems(List<BillingDtos.LineItem> items)
    {
        if (items.Count > 100)
        {
            throw new ArgumentException("billing: too many items (max 100)");
        }

        return items.Select(i =>
        {
            if (i is null)
            {
                throw new ArgumentException("billing: item invalid");
            }

            if (string.IsNullOrWhiteSpace(i.Name))
            {
                throw new ArgumentException("billing: item name required");
            }

            if (i.Name.Length > 200)
            {
                throw new ArgumentException("billing: item name too long (max 200 chars)");
            }

            if (i.UnitAmountCents <= 0)
            {
                throw new ArgumentException("billing: unitAmountCents must be > 0");
            }

            if (i.UnitAmountCents > 100_000_000L)
            {
                throw new ArgumentException("billing: unitAmountCents too large");
            }

            if (i.Quantity <= 0)
            {
                throw new ArgumentException("billing: quantity must be > 0");
            }

            if (i.Quantity > 10_000)
            {
                throw new ArgumentException("billing: quantity too large (max 10000)");
            }

            return new BillingDtos.LineItem(i.Name.Trim(), i.UnitAmountCents, i.Quantity);
        }).ToList();
    }

}

public sealed class BillingEventService(AcmeDbContext db)
{
    public async Task<BillingEventDtos.PageResponse> PageAsync(
        int page,
        int size,
        string? provider,
        string? eventType,
        DateTime? receivedFrom,
        DateTime? receivedTo,
        CancellationToken ct = default)
    {
        var query = db.BillingEvents.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(provider))
        {
            query = query.Where(x => x.Provider == provider);
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(x => x.EventType == eventType);
        }

        if (receivedFrom.HasValue)
        {
            query = query.Where(x => x.ReceivedAt >= DomainValidators.EnsureUtc(receivedFrom.Value));
        }

        if (receivedTo.HasValue)
        {
            query = query.Where(x => x.ReceivedAt <= DomainValidators.EnsureUtc(receivedTo.Value));
        }

        var total = await query.LongCountAsync(ct);

        var rows = await query
            .OrderByDescending(x => x.ReceivedAt)
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        var mapped = rows.Select(x => new BillingEventDtos.BillingEventRow(
            x.Id,
            x.Provider,
            x.EventId,
            x.EventType,
            TemporalFormat.DatetimeLocal(x.ReceivedAt))).ToList();

        return new BillingEventDtos.PageResponse(mapped, page, size, total);
    }
}

public sealed class StripePaymentService(IOptions<StripeOptions> stripeOptions, StripeGateway stripeGateway)
{
    public async Task<StripePaymentDtos.CreatePaymentIntentResponse> CreateAsync(
        StripePaymentDtos.CreatePaymentIntentRequest req,
        CancellationToken ct = default)
    {
        if (req is null)
        {
            throw new ArgumentException("Request required");
        }

        if (string.IsNullOrWhiteSpace(req.Currency))
        {
            throw new ArgumentException("currency required");
        }

        if (req.AmountCents <= 0)
        {
            throw new ArgumentException("amountCents must be > 0");
        }

        if (req.AmountCents > 2_000_000_00L)
        {
            throw new ArgumentException("amountCents too large");
        }

        var publishableKey = stripeOptions.Value.PublishableKey;

        if (string.IsNullOrWhiteSpace(publishableKey))
        {
            throw new InvalidOperationException("stripe.publishable-key not configured");
        }

        if (stripeGateway.CanCreatePaymentIntent)
        {
            return await stripeGateway.CreatePaymentIntentAsync(req, ct);
        }

        var id = $"pi_noop_{Guid.NewGuid():N}";
        var secret = $"pi_noop_secret_{Guid.NewGuid():N}";

        return new StripePaymentDtos.CreatePaymentIntentResponse(
            "stripe",
            publishableKey,
            id,
            secret,
            "requires_payment_method");
    }
}

public sealed partial class RoleService(AcmeDbContext db)
{
    private static readonly ISet<string> CanonicalPermissionCodes = PermissionCatalog.AllCodes();

    public async Task<PagedResult<RoleDtos.RoleDto>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var total = await db.AuthRoles.LongCountAsync(ct);

        var roles = await db.AuthRoles
            .AsNoTracking()
            .Include(x => x.Permissions)
            .OrderBy(x => x.Name)
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<RoleDtos.RoleDto>(
            roles.Select(ToDto).ToList(),
            page,
            size,
            total);
    }

    public async Task<RoleDtos.RoleDto> CreateAsync(RoleDtos.CreateRoleRequest reqRaw, CancellationToken ct = default)
    {
        var req = reqRaw.Normalized();

        if (await db.AuthRoles.AnyAsync(x => x.Name == req.Code, ct))
        {
            throw new ArgumentException("role.code already exists");
        }

        ValidateRoleName(req.Code);

        var permissions = await ResolvePermissionsAsync(req.PermissionCodes, ct);

        var role = new AuthRoleEntity
        {
            Id = Guid.NewGuid(),
            Name = req.Code,
            Description = req.Title,
            Permissions = permissions
        };

        db.AuthRoles.Add(role);
        await db.SaveChangesAsync(ct);

        await db.Entry(role).Collection(x => x.Permissions).LoadAsync(ct);

        return ToDto(role);
    }

    public async Task<RoleDtos.RoleDto> UpdateAsync(Guid id, RoleDtos.UpdateRoleRequest reqRaw, CancellationToken ct = default)
    {
        var req = reqRaw.Normalized();

        var role = await db.AuthRoles
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("role not found");

        if (!string.Equals(role.Name, req.Code, StringComparison.Ordinal) && await db.AuthRoles.AnyAsync(x => x.Name == req.Code, ct))
        {
            throw new ArgumentException("role.code already exists");
        }

        ValidateRoleName(req.Code);

        var permissions = await ResolvePermissionsAsync(req.PermissionCodes, ct);

        role.Name = req.Code;
        role.Description = req.Title;
        role.Permissions = permissions;

        await db.SaveChangesAsync(ct);

        return ToDto(role);
    }

    public async Task<AuthRoleEntity> RequireRoleByNameAsync(string name, CancellationToken ct = default)
    {
        return await db.AuthRoles
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Name == name, ct)
            ?? throw new KeyNotFoundException($"role not found: {name}");
    }

    private static RoleDtos.RoleDto ToDto(AuthRoleEntity role)
    {
        var codes = role.Permissions
            .Select(x => x.Code)
            .Where(x => CanonicalPermissionCodes.Contains(x))
            .OrderBy(x => x)
            .ToList();
        return new RoleDtos.RoleDto(role.Id, role.Name, role.Description ?? string.Empty, role.CreatedAt, codes);
    }

    private async Task<HashSet<AuthPermissionEntity>> ResolvePermissionsAsync(IEnumerable<string> codes, CancellationToken ct)
    {
        var unique = codes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (unique.Any(x => !CanonicalPermissionCodes.Contains(x)))
        {
            throw new ArgumentException("unknown permission code(s)");
        }

        var found = await db.AuthPermissions
            .Where(x => unique.Contains(x.Code) && CanonicalPermissionCodes.Contains(x.Code))
            .ToListAsync(ct);
        if (found.Count != unique.Count)
        {
            throw new ArgumentException("unknown permission code(s)");
        }

        return found.ToHashSet();
    }

    private static void ValidateRoleName(string value)
    {
        if (!RoleNameRegex().IsMatch(value))
        {
            throw new ArgumentException("role.name invalid");
        }
    }

    [GeneratedRegex("^[a-zA-Z0-9][a-zA-Z0-9_\\- ]{2,59}$")]
    private static partial Regex RoleNameRegex();
}

public sealed partial class UserService(AcmeDbContext db, RoleService roleService)
{
    private static readonly ISet<string> CanonicalPermissionCodes = PermissionCatalog.AllCodes();

    public async Task<PagedResult<UserDtos.UserDto>> ListAsync(int page, int size, CancellationToken ct = default)
    {
        var total = await db.AuthUsers.LongCountAsync(ct);

        var users = await db.AuthUsers
            .AsNoTracking()
            .Include(x => x.Roles)
            .ThenInclude(x => x.Permissions)
            .OrderBy(x => x.Username)
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<UserDtos.UserDto>(
            users.Select(ToDto).ToList(),
            page,
            size,
            total);
    }

    public async Task<UserDtos.UserDto> CreateAsync(UserDtos.CreateUserRequest reqRaw, CancellationToken ct = default)
    {
        var req = reqRaw.Normalized();

        ValidateEmail(req.Email);
        ValidateUsername(req.Username);

        if (await db.AuthUsers.AnyAsync(x => x.Email == req.Email, ct))
        {
            throw new ArgumentException("user.email already exists");
        }

        if (await db.AuthUsers.AnyAsync(x => x.Username == req.Username, ct))
        {
            throw new ArgumentException("user.username already exists");
        }

        var roles = await ResolveRolesAsync(req.RoleNames, ct);

        var user = new AuthUserEntity
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            Username = req.Username,
            DisplayName = req.DisplayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Status = AuthUserStatus.ACTIVE,
            Enabled = true,
            Roles = roles
        };

        db.AuthUsers.Add(user);
        await db.SaveChangesAsync(ct);

        await db.Entry(user).Collection(x => x.Roles).LoadAsync(ct);
        foreach (var role in user.Roles)
        {
            await db.Entry(role).Collection(x => x.Permissions).LoadAsync(ct);
        }

        return ToDto(user);
    }

    public async Task<UserDtos.UserDto> UpdateAsync(Guid id, UserDtos.UpdateUserRequest reqRaw, CancellationToken ct = default)
    {
        var req = reqRaw.Normalized();

        ValidateEmail(req.Email);
        ValidateUsername(req.Username);

        var user = await db.AuthUsers
            .Include(x => x.Roles)
            .ThenInclude(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("user not found");

        if (!string.Equals(user.Email, req.Email, StringComparison.OrdinalIgnoreCase) &&
            await db.AuthUsers.AnyAsync(x => x.Email == req.Email, ct))
        {
            throw new ArgumentException("user.email already exists");
        }

        if (!string.Equals(user.Username, req.Username, StringComparison.Ordinal) &&
            await db.AuthUsers.AnyAsync(x => x.Username == req.Username, ct))
        {
            throw new ArgumentException("user.username already exists");
        }

        if (!Enum.TryParse<AuthUserStatus>(req.Status, ignoreCase: true, out var status))
        {
            throw new ArgumentException("user.status invalid");
        }

        user.Email = req.Email;
        user.Username = req.Username;
        user.DisplayName = req.DisplayName;
        user.Status = status;
        user.Roles = await ResolveRolesAsync(req.RoleNames, ct);

        await db.SaveChangesAsync(ct);

        return ToDto(user);
    }

    public async Task<UserDtos.ProfileDto> ProfileByUsernameAsync(string username, CancellationToken ct = default)
    {
        var user = await db.AuthUsers
            .AsNoTracking()
            .Include(x => x.Roles)
            .ThenInclude(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Username == username, ct)
            ?? throw new KeyNotFoundException("user not found");

        var roleNames = user.Roles.Select(x => x.Name).OrderBy(x => x).ToList();
        var permissionCodes = user.Roles
            .SelectMany(x => x.Permissions)
            .Select(x => x.Code)
            .Where(x => CanonicalPermissionCodes.Contains(x))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x)
            .ToList();

        return new UserDtos.ProfileDto(
            user.Id,
            user.Email,
            user.Username,
            user.DisplayName,
            roleNames,
            permissionCodes);
    }

    public async Task MarkLoginAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.AuthUsers.FirstOrDefaultAsync(x => x.Id == userId, ct)
            ?? throw new KeyNotFoundException("user not found");

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task<HashSet<AuthRoleEntity>> ResolveRolesAsync(IEnumerable<string> roleNames, CancellationToken ct)
    {
        var unique = roleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var result = new HashSet<AuthRoleEntity>();
        foreach (var roleName in unique)
        {
            result.Add(await roleService.RequireRoleByNameAsync(roleName, ct));
        }

        return result;
    }

    private static UserDtos.UserDto ToDto(AuthUserEntity user)
    {
        var roles = user.Roles.Select(x => x.Name).OrderBy(x => x).ToList();
        var permissions = user.Roles
            .SelectMany(x => x.Permissions)
            .Select(x => x.Code)
            .Where(x => CanonicalPermissionCodes.Contains(x))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x)
            .ToList();

        return new UserDtos.UserDto(
            user.Id,
            user.Email,
            user.Username,
            user.DisplayName,
            user.Status.ToString(),
            user.CreatedAt,
            roles,
            permissions);
    }

    private static void ValidateEmail(string value)
    {
        if (!EmailRegex().IsMatch(value))
        {
            throw new ArgumentException("user.email invalid");
        }
    }

    private static void ValidateUsername(string value)
    {
        if (!UsernameRegex().IsMatch(value))
        {
            throw new ArgumentException("user.username invalid");
        }
    }

    [GeneratedRegex("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$")]
    private static partial Regex EmailRegex();

    [GeneratedRegex("^[a-zA-Z0-9][a-zA-Z0-9_\\-]{2,59}$")]
    private static partial Regex UsernameRegex();
}
