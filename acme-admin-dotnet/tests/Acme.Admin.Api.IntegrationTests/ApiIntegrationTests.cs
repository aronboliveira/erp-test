namespace Acme.Admin.Api.IntegrationTests;

[Collection("integration")]
public sealed class ApiIntegrationTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task StrictAuthRejectsAnonymousRequests()
    {
        var response = await fixture.Client.GetAsync("/api/finance/revenue");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MockHeaderAuthenticationCanBeDisabledByConfiguration()
    {
        using var noMockFactory = fixture.CreateFactoryWithOverrides(new Dictionary<string, string?>
        {
            ["Auth:EnableMockHeader"] = "false"
        });
        using var noMockClient = noMockFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var mockRequest = HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/taxes", "taxes.read");
        var response = await noMockClient.SendAsync(mockRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MockHeaderAuthenticationIsBlockedInProductionEnvironment()
    {
        using var productionFactory = fixture.CreateFactoryWithOverrides(
            new Dictionary<string, string?>
            {
                ["Auth:EnableMockHeader"] = "true"
            },
            environment: "Production");
        using var productionClient = productionFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var mockRequest = HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/taxes", "taxes.read");
        var response = await productionClient.SendAsync(mockRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task BasicAuthTakesPrecedenceOverMockHeadersWhenBothAreProvided()
    {
        var viewerUser = $"mix_{Guid.NewGuid():N}"[..14];
        const string viewerPass = "ViewerMix!123";

        var createViewer = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/admin/users", "users.write", new
        {
            email = $"{viewerUser}@acme.local",
            username = viewerUser,
            displayName = "Mixed Auth Viewer",
            password = viewerPass,
            roleNames = new[] { "VIEWER" }
        });
        var createViewerResponse = await fixture.Client.SendAsync(createViewer);
        Assert.Equal(HttpStatusCode.OK, createViewerResponse.StatusCode);

        var mixedRequest = new HttpRequestMessage(HttpMethod.Post, "/api/finance/revenue")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { occurredAt = DateTime.UtcNow, amount = 99.0m, currency = "USD", sourceRef = "mixed-auth" }),
                Encoding.UTF8,
                "application/json")
        };
        mixedRequest.Headers.Authorization = HttpTestHelpers.Basic(viewerUser, viewerPass);
        mixedRequest.Headers.Add("X-Mock-User", "integration-admin");
        mixedRequest.Headers.Add("X-Mock-Perms", "finance.write");

        var response = await fixture.Client.SendAsync(mixedRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CanonicalPermissionPoliciesGateEndpoints()
    {
        var deniedTaxes = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/taxes", "finance.read"));
        Assert.Equal(HttpStatusCode.Forbidden, deniedTaxes.StatusCode);

        var allowed = new (string Path, string Perms)[]
        {
            ("/api/taxes", "taxes.read"),
            ("/api/sales/orders", "orders.read"),
            ("/api/procurement/purchases", "procurement.read"),
            ("/api/hr/hirings", "hr.read"),
            ("/api/finance/revenue", "finance.read"),
            ("/api/catalog/items", "catalog.read")
        };

        foreach (var (path, perms) in allowed)
        {
            var response = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, path, perms));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task LegacyPermissionAliasesNoLongerAuthorizeEndpoints()
    {
        var checks = new (string Path, string LegacyPerm, string CanonicalPerm)[]
        {
            ("/api/taxes", "TAXES_READ", "taxes.read"),
            ("/api/sales/orders", "ORDERS_READ", "orders.read"),
            ("/api/admin/users", "USERS_READ", "users.read"),
            ("/api/admin/roles", "ROLES_READ", "roles.read"),
            ("/api/billing/events", "SETTINGS_READ", "billing.read")
        };

        foreach (var (path, legacy, canonical) in checks)
        {
            var legacyResponse = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, path, legacy));
            Assert.Equal(HttpStatusCode.Forbidden, legacyResponse.StatusCode);

            var canonicalResponse = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, path, canonical));
            Assert.Equal(HttpStatusCode.OK, canonicalResponse.StatusCode);
        }
    }

    [Fact]
    public async Task RoleWritesRejectNonCanonicalPermissionsEvenIfStoredInDatabase()
    {
        var roguePermissionCode = $"legacy.rogue.{Guid.NewGuid():N}"[..30];

        await fixture.WithDbContextAsync(async db =>
        {
            db.AuthPermissions.Add(new AuthPermissionEntity
            {
                Id = Guid.NewGuid(),
                Code = roguePermissionCode,
                Description = "Injected legacy compatibility permission"
            });
            await db.SaveChangesAsync();
        });

        var request = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/admin/roles", "roles.write", new
        {
            code = $"ROGUE_{Guid.NewGuid():N}"[..16],
            title = "Rogue Role",
            permissionCodes = new[] { roguePermissionCode }
        });

        var response = await fixture.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal("bad_request", body["error"]?.GetValue<string>());
    }

    [Fact]
    public async Task NonCanonicalPermissionsAreFilteredFromRoleAndProfileResponses()
    {
        var roguePermissionCode = $"legacy.rogue.{Guid.NewGuid():N}"[..30];
        var roleName = $"ROGUE_{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        var username = $"rogue_{Guid.NewGuid():N}"[..14];
        const string password = "RoguePass!123";

        await fixture.WithDbContextAsync(async db =>
        {
            var roguePermission = new AuthPermissionEntity
            {
                Id = Guid.NewGuid(),
                Code = roguePermissionCode,
                Description = "Injected compatibility alias"
            };

            var rogueRole = new AuthRoleEntity
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                Description = "Rogue role for compatibility filtering test"
            };
            rogueRole.Permissions.Add(roguePermission);

            var user = new AuthUserEntity
            {
                Id = Guid.NewGuid(),
                Email = $"{username}@acme.local",
                Username = username,
                DisplayName = "Rogue User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Status = AuthUserStatus.ACTIVE,
                Enabled = true
            };
            user.Roles.Add(rogueRole);

            db.AuthPermissions.Add(roguePermission);
            db.AuthRoles.Add(rogueRole);
            db.AuthUsers.Add(user);

            await db.SaveChangesAsync();
        });

        var rolesResponse = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/admin/roles", "roles.read"));
        Assert.Equal(HttpStatusCode.OK, rolesResponse.StatusCode);

        var roles = JsonNode.Parse(await rolesResponse.Content.ReadAsStringAsync())!
            .AsObject()["items"]!
            .AsArray()
            .Select(x => x!.AsObject())
            .ToList();

        var rogueRoleDto = roles.FirstOrDefault(x => string.Equals(x["code"]?.GetValue<string>(), roleName, StringComparison.Ordinal));
        Assert.NotNull(rogueRoleDto);

        var rolePermissionCodes = rogueRoleDto!["permissionCodes"]!
            .AsArray()
            .Select(x => x!.GetValue<string>())
            .ToList();
        Assert.DoesNotContain(roguePermissionCode, rolePermissionCodes);

        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        meRequest.Headers.Authorization = HttpTestHelpers.Basic(username, password);

        var meResponse = await fixture.Client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var mePermissions = JsonNode.Parse(await meResponse.Content.ReadAsStringAsync())!
            .AsObject()["permissionCodes"]!
            .AsArray()
            .Select(x => x!.GetValue<string>())
            .ToList();
        Assert.DoesNotContain(roguePermissionCode, mePermissions);
    }

    [Fact]
    public async Task ListEndpointsReturnPagedResultContract()
    {
        var response = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/taxes?page=0&size=2", "taxes.read"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        var items = json["items"]!.AsArray();

        Assert.Equal(0, json["page"]!.GetValue<int>());
        Assert.Equal(2, json["size"]!.GetValue<int>());
        Assert.True(json["total"]!.GetValue<long>() >= items.Count);
    }

    [Fact]
    public async Task ListEndpointsRejectInvalidPaginationQueries()
    {
        var invalidPage = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/taxes?page=-1&size=10", "taxes.read"));
        Assert.Equal(HttpStatusCode.BadRequest, invalidPage.StatusCode);

        var invalidSize = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/taxes?page=0&size=101", "taxes.read"));
        Assert.Equal(HttpStatusCode.BadRequest, invalidSize.StatusCode);
    }

    [Fact]
    public async Task CreateEndpointsRejectMissingRequiredValueTypeFields()
    {
        const string perms = "catalog.read,catalog.write,finance.write,orders.write";

        var categoriesResponse = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/catalog/categories", perms));
        Assert.Equal(HttpStatusCode.OK, categoriesResponse.StatusCode);
        var categories = JsonNode.Parse(await categoriesResponse.Content.ReadAsStringAsync())!
            .AsObject()["items"]!
            .AsArray();
        var categoryId = categories.Select(x => x!["id"]?.GetValue<string>()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        Assert.False(string.IsNullOrWhiteSpace(categoryId));

        var missingRevenueOccurredAt = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/finance/revenue", perms, new
        {
            amount = 10.5m,
            currency = "USD"
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await fixture.Client.SendAsync(missingRevenueOccurredAt)).StatusCode);

        var missingRevenueAmount = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/finance/revenue", perms, new
        {
            occurredAt = DateTime.UtcNow,
            currency = "USD"
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await fixture.Client.SendAsync(missingRevenueAmount)).StatusCode);

        var missingBudgetDates = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/finance/budgets", perms, new
        {
            plannedAmount = 1000m,
            currency = "USD"
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await fixture.Client.SendAsync(missingBudgetDates)).StatusCode);

        var missingOrderTotal = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/sales/orders", perms, new
        {
            code = "ORD-MISSING-TOTAL",
            occurredAt = DateTime.UtcNow,
            currency = "USD"
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await fixture.Client.SendAsync(missingOrderTotal)).StatusCode);

        var missingItemPrice = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/catalog/items", perms, new
        {
            kind = "PRODUCT",
            name = "Missing Price",
            currency = "USD",
            categoryId
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await fixture.Client.SendAsync(missingItemPrice)).StatusCode);
    }

    [Fact]
    public async Task CanProvisionBasicAuthUserAndAccessAdminEndpoints()
    {
        var username = $"itest_{Guid.NewGuid():N}"[..14];
        var password = "ITestPass!123";

        var createUser = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/admin/users", "users.write", new
        {
            email = $"{username}@acme.local",
            username,
            displayName = "Integration Test User",
            password,
            roleNames = new[] { "SuperAdmin" }
        });

        var createResponse = await fixture.Client.SendAsync(createUser);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        meRequest.Headers.Authorization = HttpTestHelpers.Basic(username, password);

        var meResponse = await fixture.Client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var meJson = JsonNode.Parse(await meResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(username, meJson["username"]?.GetValue<string>());

        var rolesRequest = new HttpRequestMessage(HttpMethod.Get, "/api/admin/roles");
        rolesRequest.Headers.Authorization = HttpTestHelpers.Basic(username, password);

        var rolesResponse = await fixture.Client.SendAsync(rolesRequest);
        Assert.Equal(HttpStatusCode.OK, rolesResponse.StatusCode);
    }

    [Fact]
    public async Task LegacySeededAdminRoleIsCanonicalizedForDotnetPolicies()
    {
        var listRoles = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/admin/roles", "roles.read"));
        Assert.Equal(HttpStatusCode.OK, listRoles.StatusCode);

        var rolesJson = JsonNode.Parse(await listRoles.Content.ReadAsStringAsync())!
            .AsObject()["items"]!
            .AsArray();
        var adminRole = rolesJson
            .Select(x => x!.AsObject())
            .FirstOrDefault(x => string.Equals(x["code"]?.GetValue<string>(), "ADMIN", StringComparison.Ordinal));

        Assert.NotNull(adminRole);

        var permissionCodes = adminRole!["permissionCodes"]!
            .AsArray()
            .Select(x => x!.GetValue<string>())
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("finance.read", permissionCodes);
        Assert.Contains("orders.read", permissionCodes);
        Assert.Contains("procurement.read", permissionCodes);
        Assert.Contains("hr.read", permissionCodes);
        Assert.Contains("users.read", permissionCodes);
        Assert.DoesNotContain("ORDERS_READ", permissionCodes);
        Assert.DoesNotContain("BILLS_READ", permissionCodes);

        var username = $"legacy_{Guid.NewGuid():N}"[..14];
        const string password = "LegacyPass!123";

        var createUser = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/admin/users", "users.write", new
        {
            email = $"{username}@acme.local",
            username,
            displayName = "Legacy Role User",
            password,
            roleNames = new[] { "ADMIN" }
        });

        var createUserResponse = await fixture.Client.SendAsync(createUser);
        Assert.Equal(HttpStatusCode.OK, createUserResponse.StatusCode);

        var procurement = new HttpRequestMessage(HttpMethod.Get, "/api/procurement/purchases");
        procurement.Headers.Authorization = HttpTestHelpers.Basic(username, password);
        var procurementResponse = await fixture.Client.SendAsync(procurement);
        Assert.Equal(HttpStatusCode.OK, procurementResponse.StatusCode);

        var hr = new HttpRequestMessage(HttpMethod.Get, "/api/hr/hirings");
        hr.Headers.Authorization = HttpTestHelpers.Basic(username, password);
        var hrResponse = await fixture.Client.SendAsync(hr);
        Assert.Equal(HttpStatusCode.OK, hrResponse.StatusCode);
    }

    [Fact]
    public async Task LegacyManagerAndViewerRolesMatchCanonicalAccessMatrix()
    {
        var listRoles = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/admin/roles", "roles.read"));
        Assert.Equal(HttpStatusCode.OK, listRoles.StatusCode);

        var roles = JsonNode.Parse(await listRoles.Content.ReadAsStringAsync())!
            .AsObject()["items"]!
            .AsArray()
            .Select(x => x!.AsObject())
            .ToList();

        var manager = roles.FirstOrDefault(x => string.Equals(x["code"]?.GetValue<string>(), "MANAGER", StringComparison.Ordinal));
        var viewer = roles.FirstOrDefault(x => string.Equals(x["code"]?.GetValue<string>(), "VIEWER", StringComparison.Ordinal));

        Assert.NotNull(manager);
        Assert.NotNull(viewer);

        var managerPerms = manager!["permissionCodes"]!.AsArray().Select(x => x!.GetValue<string>()).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("finance.write", managerPerms);
        Assert.Contains("billing.create", managerPerms);
        Assert.DoesNotContain("users.write", managerPerms);

        var viewerPerms = viewer!["permissionCodes"]!.AsArray().Select(x => x!.GetValue<string>()).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("finance.read", viewerPerms);
        Assert.DoesNotContain("finance.write", viewerPerms);
        Assert.DoesNotContain("billing.create", viewerPerms);

        var managerUser = $"mgr_{Guid.NewGuid():N}"[..14];
        const string managerPass = "ManagerPass!123";
        var createManager = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/admin/users", "users.write", new
        {
            email = $"{managerUser}@acme.local",
            username = managerUser,
            displayName = "Manager User",
            password = managerPass,
            roleNames = new[] { "MANAGER" }
        });
        var createManagerResponse = await fixture.Client.SendAsync(createManager);
        Assert.Equal(HttpStatusCode.OK, createManagerResponse.StatusCode);

        var managerReadRevenue = new HttpRequestMessage(HttpMethod.Get, "/api/finance/revenue");
        managerReadRevenue.Headers.Authorization = HttpTestHelpers.Basic(managerUser, managerPass);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.SendAsync(managerReadRevenue)).StatusCode);

        var managerWriteRevenue = new HttpRequestMessage(HttpMethod.Post, "/api/finance/revenue")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { occurredAt = DateTime.UtcNow, amount = 110.5m, currency = "USD", sourceRef = "mgr" }),
                Encoding.UTF8,
                "application/json")
        };
        managerWriteRevenue.Headers.Authorization = HttpTestHelpers.Basic(managerUser, managerPass);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.SendAsync(managerWriteRevenue)).StatusCode);

        var managerAdminUsers = new HttpRequestMessage(HttpMethod.Get, "/api/admin/users");
        managerAdminUsers.Headers.Authorization = HttpTestHelpers.Basic(managerUser, managerPass);
        Assert.Equal(HttpStatusCode.Forbidden, (await fixture.Client.SendAsync(managerAdminUsers)).StatusCode);

        var viewerUser = $"view_{Guid.NewGuid():N}"[..14];
        const string viewerPass = "ViewerPass!123";
        var createViewer = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/admin/users", "users.write", new
        {
            email = $"{viewerUser}@acme.local",
            username = viewerUser,
            displayName = "Viewer User",
            password = viewerPass,
            roleNames = new[] { "VIEWER" }
        });
        var createViewerResponse = await fixture.Client.SendAsync(createViewer);
        Assert.Equal(HttpStatusCode.OK, createViewerResponse.StatusCode);

        var viewerReadRevenue = new HttpRequestMessage(HttpMethod.Get, "/api/finance/revenue");
        viewerReadRevenue.Headers.Authorization = HttpTestHelpers.Basic(viewerUser, viewerPass);
        Assert.Equal(HttpStatusCode.OK, (await fixture.Client.SendAsync(viewerReadRevenue)).StatusCode);

        var viewerWriteRevenue = new HttpRequestMessage(HttpMethod.Post, "/api/finance/revenue")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { occurredAt = DateTime.UtcNow, amount = 95.0m, currency = "USD", sourceRef = "viewer" }),
                Encoding.UTF8,
                "application/json")
        };
        viewerWriteRevenue.Headers.Authorization = HttpTestHelpers.Basic(viewerUser, viewerPass);
        Assert.Equal(HttpStatusCode.Forbidden, (await fixture.Client.SendAsync(viewerWriteRevenue)).StatusCode);

        var viewerBillingCreate = new HttpRequestMessage(HttpMethod.Post, "/api/billing/checkout-session")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    currency = "usd",
                    items = new[] { new { name = "Plan", unitAmountCents = 9900, quantity = 1 } },
                    successUrl = "http://localhost:14000/s",
                    cancelUrl = "http://localhost:14000/c"
                }),
                Encoding.UTF8,
                "application/json")
        };
        viewerBillingCreate.Headers.Authorization = HttpTestHelpers.Basic(viewerUser, viewerPass);
        Assert.Equal(HttpStatusCode.Forbidden, (await fixture.Client.SendAsync(viewerBillingCreate)).StatusCode);
    }

    [Fact]
    public async Task LegacyNotNullColumnsNoLongerBlockCoreCreateFlows()
    {
        const string perms = "catalog.read,catalog.write,finance.read,finance.write";

        var categoriesResponse = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/catalog/categories", perms));
        Assert.Equal(HttpStatusCode.OK, categoriesResponse.StatusCode);
        var categories = JsonNode.Parse(await categoriesResponse.Content.ReadAsStringAsync())!
            .AsObject()["items"]!
            .AsArray();
        var catalogCategoryId = categories
            .Select(x => x!["id"]?.GetValue<string>())
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        Assert.False(string.IsNullOrWhiteSpace(catalogCategoryId));

        var createItem = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/catalog/items", perms, new
        {
            kind = "PRODUCT",
            name = "Schema Compatibility Item",
            sku = "SCHEMA-COMP-001",
            price = 14.90m,
            currency = "USD",
            categoryId = catalogCategoryId
        });
        var createItemResponse = await fixture.Client.SendAsync(createItem);
        Assert.Equal(HttpStatusCode.OK, createItemResponse.StatusCode);

        var expenseCategoriesResponse = await fixture.Client.SendAsync(HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/finance/expense-categories", perms));
        Assert.Equal(HttpStatusCode.OK, expenseCategoriesResponse.StatusCode);
        var expenseCategories = JsonNode.Parse(await expenseCategoriesResponse.Content.ReadAsStringAsync())!
            .AsObject()["items"]!
            .AsArray();
        var expenseCategoryId = expenseCategories
            .Select(x => x!["id"]?.GetValue<string>())
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        Assert.False(string.IsNullOrWhiteSpace(expenseCategoryId));

        var createExpense = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/finance/expenses", perms, new
        {
            occurredAt = DateTime.UtcNow,
            amount = 48.25m,
            currency = "USD",
            categoryId = expenseCategoryId,
            vendor = "Schema Vendor"
        });
        var createExpenseResponse = await fixture.Client.SendAsync(createExpense);
        Assert.Equal(HttpStatusCode.OK, createExpenseResponse.StatusCode);

        var createRevenue = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/finance/revenue", perms, new
        {
            occurredAt = DateTime.UtcNow,
            amount = 310.75m,
            currency = "USD",
            sourceRef = "schema-compat"
        });
        var createRevenueResponse = await fixture.Client.SendAsync(createRevenue);
        Assert.Equal(HttpStatusCode.OK, createRevenueResponse.StatusCode);

        var createBudget = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/finance/budgets", perms, new
        {
            periodStart = "2026-01-01",
            periodEnd = "2026-12-31",
            plannedAmount = 25000m,
            currency = "USD"
        });
        var createBudgetResponse = await fixture.Client.SendAsync(createBudget);
        Assert.Equal(HttpStatusCode.OK, createBudgetResponse.StatusCode);
    }

    [Fact]
    public async Task BillingAndWebhookFlowMatchesContract()
    {
        var checkout = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/billing/checkout-session", "billing.create", new
        {
            currency = "usd",
            items = new[]
            {
                new { name = "Plan", unitAmountCents = 9900, quantity = 1 }
            },
            successUrl = "http://localhost:14000/s",
            cancelUrl = "http://localhost:14000/c"
        });

        var checkoutResponse = await fixture.Client.SendAsync(checkout);
        Assert.Equal(HttpStatusCode.Created, checkoutResponse.StatusCode);

        var checkoutJson = JsonNode.Parse(await checkoutResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal("stripe", checkoutJson["provider"]?.GetValue<string>());
        Assert.False(string.IsNullOrWhiteSpace(checkoutJson["sessionId"]?.GetValue<string>()));

        var paymentIntent = HttpTestHelpers.MockAuthedJson(HttpMethod.Post, "/api/billing/payment-intents", "billing.pay", new
        {
            currency = "usd",
            amountCents = 1200,
            customerEmail = "test@example.com",
            description = "integration"
        });

        var paymentResponse = await fixture.Client.SendAsync(paymentIntent);
        Assert.Equal(HttpStatusCode.OK, paymentResponse.StatusCode);

        var paymentJson = JsonNode.Parse(await paymentResponse.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal("pk_test_noop", paymentJson["publishableKey"]?.GetValue<string>());

        var payload = JsonSerializer.Serialize(new
        {
            id = $"evt_{Guid.NewGuid():N}",
            type = "checkout.session.completed"
        });

        var validWebhook = new HttpRequestMessage(HttpMethod.Post, "/api/billing/webhook")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        validWebhook.Headers.Add("Stripe-Signature", HttpTestHelpers.StripeSignature(payload, "whsec_dev"));

        var okWebhookResponse = await fixture.Client.SendAsync(validWebhook);
        Assert.Equal(HttpStatusCode.OK, okWebhookResponse.StatusCode);

        var invalidWebhook = new HttpRequestMessage(HttpMethod.Post, "/api/billing/webhook")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        invalidWebhook.Headers.Add("Stripe-Signature", "t=1,v1=deadbeef");

        var invalidWebhookResponse = await fixture.Client.SendAsync(invalidWebhook);
        Assert.Equal(HttpStatusCode.BadRequest, invalidWebhookResponse.StatusCode);

        var eventsRequest = HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/billing/events?page=0&size=1&provider=stripe&eventType=checkout.session.completed", "billing.read");
        var eventsResponse = await fixture.Client.SendAsync(eventsRequest);
        Assert.Equal(HttpStatusCode.OK, eventsResponse.StatusCode);

        var eventsJson = JsonNode.Parse(await eventsResponse.Content.ReadAsStringAsync())!.AsObject();
        var items = eventsJson["items"]!.AsArray();

        Assert.True(items.Count <= 1);
        Assert.True(eventsJson["total"]!.GetValue<long>() >= 1);
        Assert.Equal(0, eventsJson["page"]!.GetValue<int>());
        Assert.Equal(1, eventsJson["size"]!.GetValue<int>());
    }

    [Fact]
    public async Task WebhookRejectsPayloadWithoutRequiredStripeEventFields()
    {
        var missingIdPayload = JsonSerializer.Serialize(new
        {
            type = "checkout.session.completed"
        });

        var missingIdRequest = new HttpRequestMessage(HttpMethod.Post, "/api/billing/webhook")
        {
            Content = new StringContent(missingIdPayload, Encoding.UTF8, "application/json")
        };
        missingIdRequest.Headers.Add("Stripe-Signature", HttpTestHelpers.StripeSignature(missingIdPayload, "whsec_dev"));

        var missingIdResponse = await fixture.Client.SendAsync(missingIdRequest);
        Assert.Equal(HttpStatusCode.BadRequest, missingIdResponse.StatusCode);

        var missingTypePayload = JsonSerializer.Serialize(new
        {
            id = $"evt_{Guid.NewGuid():N}"
        });

        var missingTypeRequest = new HttpRequestMessage(HttpMethod.Post, "/api/billing/webhook")
        {
            Content = new StringContent(missingTypePayload, Encoding.UTF8, "application/json")
        };
        missingTypeRequest.Headers.Add("Stripe-Signature", HttpTestHelpers.StripeSignature(missingTypePayload, "whsec_dev"));

        var missingTypeResponse = await fixture.Client.SendAsync(missingTypeRequest);
        Assert.Equal(HttpStatusCode.BadRequest, missingTypeResponse.StatusCode);
    }

    [Fact]
    public async Task BillingEventsPaginationNoLongerClampsSizeTo50()
    {
        await fixture.WithDbContextAsync(async db =>
        {
            for (var i = 0; i < 55; i++)
            {
                db.BillingEvents.Add(new BillingEventEntity
                {
                    Id = Guid.NewGuid(),
                    Provider = "stripe",
                    EventId = $"evt_bulk_{Guid.NewGuid():N}",
                    EventType = "checkout.session.completed",
                    Payload = "{}",
                    ReceivedAt = DateTime.UtcNow.AddSeconds(-i)
                });
            }

            await db.SaveChangesAsync();
        });

        var response = await fixture.Client.SendAsync(
            HttpTestHelpers.MockAuthed(HttpMethod.Get, "/api/billing/events?page=0&size=55", "billing.read"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal(55, json["size"]!.GetValue<int>());

        var items = json["items"]!.AsArray();
        Assert.True(items.Count <= 55);
    }

    [Fact]
    public async Task BillingEventsRejectMalformedDatetimeLocalFilters()
    {
        var malformed = await fixture.Client.SendAsync(
            HttpTestHelpers.MockAuthed(
                HttpMethod.Get,
                "/api/billing/events?receivedFrom=2026-01-01T00:00:00Z",
                "billing.read"));

        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);

        var body = JsonNode.Parse(await malformed.Content.ReadAsStringAsync())!.AsObject();
        Assert.Equal("bad_request", body["error"]?.GetValue<string>());
    }
}
