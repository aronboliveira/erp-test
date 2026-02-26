namespace Acme.Admin.Api.Domain;

public abstract class AuditedEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public abstract class TaxLinkedEntity : AuditedEntity
{
    public List<Guid>? TaxIds { get; set; }
}

public enum ProductKind
{
    PRODUCT,
    SERVICE
}

public enum ExpenseSubject
{
    ORDER,
    PURCHASE,
    TAX,
    HIRING,
    BILL,
    INVOICE
}

public enum AuthUserStatus
{
    ACTIVE,
    SUSPENDED,
    DISABLED
}

public sealed class TaxEntity : AuditedEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool Enabled { get; set; } = true;
}

public sealed class ProductOrServiceCategoryEntity : AuditedEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class ProductOrServiceEntity : AuditedEntity
{
    public Guid Id { get; set; }
    public ProductKind Kind { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";

    public Guid CategoryId { get; set; }
    public ProductOrServiceCategoryEntity? Category { get; set; }
}

public sealed class ExpenseCategoryEntity : AuditedEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ExpenseSubject Subject { get; set; } = ExpenseSubject.ORDER;
    public string? Description { get; set; }
}

public sealed class ExpenseEntity : AuditedEntity
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";

    public Guid CategoryId { get; set; }
    public ExpenseCategoryEntity? Category { get; set; }

    public string? Vendor { get; set; }
}

public sealed class RevenueEntity : AuditedEntity
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? SourceRef { get; set; }
}

public sealed class BudgetEntity : AuditedEntity
{
    public Guid Id { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public decimal PlannedAmount { get; set; }
    public string Currency { get; set; } = "USD";
}

public sealed class OrderEntity : TaxLinkedEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Total { get; set; }
}

public sealed class PurchaseEntity : TaxLinkedEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Total { get; set; }
    public string? Vendor { get; set; }
}

public sealed class BillEntity : TaxLinkedEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? DueAt { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Total { get; set; }
    public string? Vendor { get; set; }
    public string? Payee { get; set; }
}

public sealed class HiringEntity : TaxLinkedEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public decimal GrossSalary { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? Total { get; set; }
    public string? CandidateName { get; set; }
}

public sealed class BillingEventEntity
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public string Payload { get; set; } = string.Empty;
}

public sealed class AuthPermissionEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<AuthRoleEntity> Roles { get; set; } = new HashSet<AuthRoleEntity>();
}

public sealed class AuthRoleEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<AuthPermissionEntity> Permissions { get; set; } = new HashSet<AuthPermissionEntity>();
    public ICollection<AuthUserEntity> Users { get; set; } = new HashSet<AuthUserEntity>();
}

public sealed class AuthUserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public AuthUserStatus Status { get; set; } = AuthUserStatus.ACTIVE;
    public DateTime? LastLoginAt { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<AuthRoleEntity> Roles { get; set; } = new HashSet<AuthRoleEntity>();
}
