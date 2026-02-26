using Microsoft.AspNetCore.Authorization;

namespace Acme.Admin.Api.Security;

public static class PermissionPolicies
{
    public const string PermissionClaim = "perm";

    public const string CatalogRead = "catalog.read";
    public const string CatalogWrite = "catalog.write";

    public const string BillingCreate = "billing.create";
    public const string BillingRead = "billing.read";
    public const string BillingPay = "billing.pay";

    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string RolesRead = "roles.read";
    public const string RolesWrite = "roles.write";

    public const string TaxesRead = "taxes.read";
    public const string TaxesWrite = "taxes.write";

    public const string OrdersRead = "orders.read";
    public const string OrdersWrite = "orders.write";

    public const string ProcurementRead = "procurement.read";
    public const string ProcurementWrite = "procurement.write";

    public const string FinanceRead = "finance.read";
    public const string FinanceWrite = "finance.write";

    public const string HrRead = "hr.read";
    public const string HrWrite = "hr.write";

    public static void Register(AuthorizationOptions options)
    {
        AddPermission(options, CatalogRead);
        AddPermission(options, CatalogWrite);
        AddPermission(options, BillingCreate);
        AddPermission(options, BillingRead);
        AddPermission(options, BillingPay);
        AddPermission(options, UsersRead);
        AddPermission(options, UsersWrite);
        AddPermission(options, RolesRead);
        AddPermission(options, RolesWrite);
        AddPermission(options, TaxesRead);
        AddPermission(options, TaxesWrite);
        AddPermission(options, OrdersRead);
        AddPermission(options, OrdersWrite);
        AddPermission(options, ProcurementRead);
        AddPermission(options, ProcurementWrite);
        AddPermission(options, FinanceRead);
        AddPermission(options, FinanceWrite);
        AddPermission(options, HrRead);
        AddPermission(options, HrWrite);
    }

    private static void AddPermission(AuthorizationOptions options, string permission)
    {
        options.AddPolicy(permission, policy =>
            policy.RequireClaim(PermissionClaim, permission));
    }
}
