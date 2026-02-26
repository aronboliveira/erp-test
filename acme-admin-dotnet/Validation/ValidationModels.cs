using System.Globalization;

namespace Acme.Admin.Api.Validation;

public enum Severity
{
    LOG,
    INFO,
    WARN,
    ERROR
}

public sealed record ValidationIssue(
    string Field,
    string Reason,
    Severity Severity,
    IReadOnlyDictionary<string, object> Meta);

public sealed class ValidationResult
{
    private readonly List<ValidationIssue> _issues = [];

    public ValidationResult Add(ValidationIssue issue)
    {
        if (issue is not null)
        {
            _issues.Add(issue);
        }

        return this;
    }

    public ValidationResult AddRange(IEnumerable<ValidationIssue> issues)
    {
        foreach (var issue in issues)
        {
            Add(issue);
        }

        return this;
    }

    public bool HasErrors()
    {
        return _issues.Any(x => x.Severity == Severity.ERROR);
    }

    public IReadOnlyList<ValidationIssue> ToList() => _issues;
}

public sealed class ApiValidationException(IReadOnlyList<ValidationIssue> issues) : Exception("Validation failed")
{
    public IReadOnlyList<ValidationIssue> Issues { get; } = issues;
}

public sealed record OccurredAtPolicy(bool BusinessDaysOnly, TimeSpan MaxAge, TimeSpan FutureSkew);
public sealed record TaxIdsPolicy(int MaxItems, bool AllowEmpty);

public static class DateValidator
{
    private const string DatetimeLocalFormat = "yyyy-MM-dd'T'HH:mm";

    public static bool IsDatetimeLocal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return DateTime.TryParseExact(
            value.Trim(),
            DatetimeLocalFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
    }

    public static DateTime ParseDatetimeLocal(string value)
    {
        if (!IsDatetimeLocal(value))
        {
            throw new ArgumentException("Invalid datetime-local");
        }

        return DateTime.ParseExact(value.Trim(), DatetimeLocalFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    public static void AssertRange(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && to.Value < from.Value)
        {
            throw new ArgumentException("Invalid range: to < from");
        }
    }

    public static bool IsFuture(DateTime atUtc, DateTime nowUtc, TimeSpan skew)
    {
        return atUtc > nowUtc.Add(skew);
    }

    public static bool IsTooOld(DateTime atUtc, DateTime nowUtc, TimeSpan maxAge)
    {
        return atUtc < nowUtc.Subtract(maxAge);
    }

    public static DateOnly ToUtcDate(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        return DateOnly.FromDateTime(utc);
    }
}

public static class TemporalFormat
{
    private const string DatetimeLocalFormat = "yyyy-MM-dd'T'HH:mm";

    public static string DatetimeLocal(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        return utc.ToString(DatetimeLocalFormat, CultureInfo.InvariantCulture);
    }
}

public static class DomainValidators
{
    public static ValidationResult ValidateTaxIdsShape(IEnumerable<Guid?>? taxIds, TaxIdsPolicy policy)
    {
        var result = new ValidationResult();

        if (taxIds is null)
        {
            if (!policy.AllowEmpty)
            {
                result.Add(new ValidationIssue(
                    "taxIds",
                    "taxIds is required (can be empty list)",
                    Severity.ERROR,
                    new Dictionary<string, object>()));
            }

            return result;
        }

        var list = taxIds.ToList();
        if (list.Count == 0 && !policy.AllowEmpty)
        {
            result.Add(new ValidationIssue(
                "taxIds",
                "taxIds cannot be empty",
                Severity.ERROR,
                new Dictionary<string, object>()));
            return result;
        }

        if (list.Count > policy.MaxItems)
        {
            result.Add(new ValidationIssue(
                "taxIds",
                "taxIds exceeds maxItems",
                Severity.ERROR,
                new Dictionary<string, object> { ["maxItems"] = policy.MaxItems }));
        }

        var seen = new HashSet<Guid>();
        foreach (var taxId in list)
        {
            if (!taxId.HasValue)
            {
                result.Add(new ValidationIssue(
                    "taxIds",
                    "taxIds contains null",
                    Severity.ERROR,
                    new Dictionary<string, object>()));
                continue;
            }

            if (!seen.Add(taxId.Value))
            {
                result.Add(new ValidationIssue(
                    "taxIds",
                    "taxIds contains duplicates",
                    Severity.WARN,
                    new Dictionary<string, object> { ["taxId"] = taxId.Value.ToString() }));
            }
        }

        return result;
    }

    public static void ValidateOccurredAt(
        ValidationResult result,
        string field,
        DateTime? occurredAt,
        DateTime nowUtc,
        OccurredAtPolicy policy)
    {
        if (!occurredAt.HasValue)
        {
            result.Add(new ValidationIssue(field, $"{field} is required", Severity.ERROR, new Dictionary<string, object>()));
            return;
        }

        var utc = EnsureUtc(occurredAt.Value);

        if (DateValidator.IsFuture(utc, nowUtc, policy.FutureSkew))
        {
            result.Add(new ValidationIssue(
                field,
                $"{field} cannot be in the future",
                Severity.ERROR,
                new Dictionary<string, object>()));
        }

        if (DateValidator.IsTooOld(utc, nowUtc, policy.MaxAge))
        {
            result.Add(new ValidationIssue(
                field,
                $"{field} exceeds maxAge",
                Severity.ERROR,
                new Dictionary<string, object> { ["maxAge"] = policy.MaxAge.ToString() }));
        }

        if (policy.BusinessDaysOnly)
        {
            var d = DateValidator.ToUtcDate(utc);
            var day = d.DayOfWeek;
            if (day is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                result.Add(new ValidationIssue(
                    field,
                    $"{field} must be a business day (Mon-Fri)",
                    Severity.WARN,
                    new Dictionary<string, object> { ["date"] = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }));
            }
        }
    }

    public static DateTime EnsureUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            return value;
        }

        if (value.Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        return value.ToUniversalTime();
    }

    public static List<Guid>? NormalizeTaxIds(List<Guid>? ids, int maxItems = 64)
    {
        if (ids is null)
        {
            return null;
        }

        if (ids.Count == 0)
        {
            return [];
        }

        var result = new List<Guid>(Math.Min(ids.Count, maxItems));
        var seen = new HashSet<Guid>();

        foreach (var id in ids)
        {
            if (seen.Add(id))
            {
                result.Add(id);
            }

            if (result.Count >= maxItems)
            {
                break;
            }
        }

        return result;
    }
}
