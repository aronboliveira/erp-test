namespace Acme.Admin.Api.Security;

public static class PermissionCatalog
{
    public sealed record PermissionSpec(string Code, string Description);

    public static IReadOnlyList<PermissionSpec> Specs() =>
    [
        // Canonical .NET policy permission catalog.
        new("users.read", "List/read users"),
        new("users.write", "Create/update users"),
        new("roles.read", "List/read roles"),
        new("roles.write", "Create/update roles"),
        new("taxes.read", "List/read taxes"),
        new("taxes.write", "Create/update/delete taxes"),
        new("orders.read", "List/read orders"),
        new("orders.write", "Create/update orders"),
        new("procurement.read", "List/read procurement data"),
        new("procurement.write", "Create/update procurement data"),
        new("finance.read", "Read finance data"),
        new("finance.write", "Write finance data"),
        new("hr.read", "Read HR data"),
        new("hr.write", "Write HR data"),
        new("catalog.read", "Read catalog"),
        new("catalog.write", "Write catalog"),
        new("billing.read", "Read billing data"),
        new("billing.create", "Create billing sessions"),
        new("billing.pay", "Pay billing intents")
    ];

    public static ISet<string> AllCodes()
    {
        return Specs().Select(x => x.Code).ToHashSet(StringComparer.Ordinal);
    }
}
