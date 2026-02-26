using System.ComponentModel.DataAnnotations;
using Acme.Admin.Api.Domain;

namespace Acme.Admin.Api.DTO;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Size,
    long Total);

public static class TaxDtos
{
    public sealed record CreateTaxRequest(
        string? Code,
        string? Title,
        decimal? Rate);

    public sealed record UpdateTaxRequest(
        string? Code,
        string? Title,
        decimal? Rate);

    public sealed record TaxResponse(
        Guid Id,
        string Code,
        string Title,
        decimal Rate,
        string CreatedAt);
}

public sealed record ProductOrServiceCreateRequest(
    [Required] ProductKind? Kind,
    [Required, MaxLength(180)] string Name,
    [MaxLength(64)] string? Sku,
    [Required, Range(typeof(decimal), "0.00", "999999999999.99")] decimal? Price,
    [Required, MinLength(3), MaxLength(3)] string Currency,
    [Required] Guid? CategoryId);

public sealed record ProductOrServiceResponse(
    Guid Id,
    ProductKind Kind,
    string Name,
    string? Sku,
    string Price,
    string Currency,
    Guid? CategoryId);

public sealed record ProductOrServiceCategoryCreateRequest(
    [Required, MaxLength(64)] string Code,
    [Required, MaxLength(180)] string Name,
    string? Description);

public sealed record ExpenseCategoryCreateRequest(
    [Required, MaxLength(64)] string Code,
    [Required, MaxLength(180)] string Name,
    [Required] ExpenseSubject Subject,
    string? Description);

public sealed record ExpenseCreateRequest(
    [Required] DateTime? OccurredAt,
    [Required, Range(typeof(decimal), "0.00", "999999999999.99")] decimal? Amount,
    [Required, MinLength(3), MaxLength(3)] string Currency,
    [Required] Guid? CategoryId,
    string? Vendor);

public sealed record ExpenseResponse(
    Guid Id,
    string OccurredAt,
    string Amount,
    string Currency,
    Guid? CategoryId,
    string? Vendor);

public sealed record RevenueCreateRequest(
    [Required] DateTime? OccurredAt,
    [Required, Range(typeof(decimal), "0.00", "999999999999.99")] decimal? Amount,
    [Required, MinLength(3), MaxLength(3)] string Currency,
    string? SourceRef);

public sealed record BudgetCreateRequest(
    [Required] DateOnly? PeriodStart,
    [Required] DateOnly? PeriodEnd,
    [Required, Range(typeof(decimal), "0.00", "999999999999.99")] decimal? PlannedAmount,
    [Required, MinLength(3), MaxLength(3)] string Currency);

public sealed record BillCreateRequest(
    string? Code,
    DateTime? OccurredAt,
    DateTime? DueAt,
    string? Currency,
    decimal? Total,
    string? Vendor,
    string? Payee,
    List<Guid>? TaxIds);

public sealed record HiringCreateRequest(
    string? Code,
    DateTime? OccurredAt,
    string? EmployeeName,
    string? Role,
    DateTime? StartAt,
    DateTime? EndAt,
    decimal? GrossSalary,
    string? Currency,
    decimal? Total,
    string? CandidateName,
    List<Guid>? TaxIds);

public sealed record PurchaseCreateRequest(
    [Required, MaxLength(64)] string Code,
    [Required] DateTime? OccurredAt,
    [Required, MinLength(3), MaxLength(3)] string Currency,
    [Required, Range(typeof(decimal), "0.00", "999999999999.99")] decimal? Total,
    [MaxLength(180)] string? Vendor,
    List<Guid>? TaxIds);

public sealed record OrderCreateRequest(
    [Required, MaxLength(64)] string Code,
    [Required] DateTime? OccurredAt,
    [Required, MinLength(3), MaxLength(3)] string Currency,
    [Required, Range(typeof(decimal), "0.00", "999999999999.99")] decimal? Total,
    List<Guid>? TaxIds);

public static class BillingDtos
{
    public sealed record CreateCheckoutSessionRequest(
        string? Currency,
        string? CustomerEmail,
        List<LineItem>? Items,
        string? SuccessUrl,
        string? CancelUrl);

    public sealed record LineItem(
        string? Name,
        long UnitAmountCents,
        long Quantity);

    public sealed record CheckoutSessionResponse(
        string Provider,
        string SessionId,
        string Url);
}

public static class BillingEventDtos
{
    public sealed record BillingEventRow(
        Guid Id,
        string Provider,
        string EventId,
        string EventType,
        string ReceivedAt);

    public sealed record PageResponse(
        IReadOnlyList<BillingEventRow> Items,
        int Page,
        int Size,
        long Total);
}

public static class StripePaymentDtos
{
    public sealed record CreatePaymentIntentRequest(
        string? Currency,
        long AmountCents,
        string? CustomerEmail,
        string? Description);

    public sealed record CreatePaymentIntentResponse(
        string Provider,
        string PublishableKey,
        string PaymentIntentId,
        string ClientSecret,
        string Status);

    public sealed record PaymentIntentResult(
        string Provider,
        string PaymentIntentId,
        string ClientSecret,
        string Status);
}

public static class RoleDtos
{
    public sealed record RoleDto(
        Guid Id,
        string Code,
        string Title,
        DateTime CreatedAt,
        IReadOnlyList<string> PermissionCodes);

    public sealed record CreateRoleRequest(
        [Required, MaxLength(60)] string Code,
        [Required, MaxLength(120)] string Title,
        [Required] List<string> PermissionCodes)
    {
        public CreateRoleRequest Normalized()
        {
            var code = Code?.Trim().ToUpperInvariant() ?? string.Empty;
            var title = Title?.Trim() ?? string.Empty;
            var permissions = PermissionCodes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            return new CreateRoleRequest(code, title, permissions);
        }
    }

    public sealed record UpdateRoleRequest(
        [Required, MaxLength(60)] string Code,
        [Required, MaxLength(120)] string Title,
        [Required] List<string> PermissionCodes)
    {
        public UpdateRoleRequest Normalized()
        {
            var code = Code?.Trim().ToUpperInvariant() ?? string.Empty;
            var title = Title?.Trim() ?? string.Empty;
            var permissions = PermissionCodes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            return new UpdateRoleRequest(code, title, permissions);
        }
    }
}

public static class UserDtos
{
    public sealed record UserDto(
        Guid Id,
        string Email,
        string Username,
        string? DisplayName,
        string Status,
        DateTime CreatedAt,
        IReadOnlyList<string> RoleNames,
        IReadOnlyList<string> PermissionCodes);

    public sealed record CreateUserRequest(
        [Required, EmailAddress, MaxLength(254)] string Email,
        [Required, MinLength(3), MaxLength(60)] string Username,
        [MaxLength(120)] string? DisplayName,
        [Required, MinLength(8), MaxLength(120)] string Password,
        [Required] List<string> RoleNames)
    {
        public CreateUserRequest Normalized()
        {
            var email = Email?.Trim().ToLowerInvariant() ?? string.Empty;
            var username = Username?.Trim() ?? string.Empty;
            var displayName = DisplayName?.Trim();
            var roleNames = RoleNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            return new CreateUserRequest(email, username, displayName, Password ?? string.Empty, roleNames);
        }
    }

    public sealed record UpdateUserRequest(
        [Required, EmailAddress, MaxLength(254)] string Email,
        [Required, MinLength(3), MaxLength(60)] string Username,
        [MaxLength(120)] string? DisplayName,
        [Required, MaxLength(20)] string Status,
        [Required] List<string> RoleNames)
    {
        public UpdateUserRequest Normalized()
        {
            var email = Email?.Trim().ToLowerInvariant() ?? string.Empty;
            var username = Username?.Trim() ?? string.Empty;
            var displayName = DisplayName?.Trim();
            var status = Status?.Trim().ToUpperInvariant() ?? string.Empty;
            var roleNames = RoleNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            return new UpdateUserRequest(email, username, displayName, status, roleNames);
        }
    }

    public sealed record ProfileDto(
        Guid Id,
        string Email,
        string Username,
        string? DisplayName,
        IReadOnlyList<string> RoleNames,
        IReadOnlyList<string> PermissionCodes);
}
