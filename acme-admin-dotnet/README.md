# ACME Admin .NET Backend

## Status

Backend transpilation from Java to .NET is complete on `main`.

Implemented and hardened in the migration:
- ASP.NET Core Web API scaffold (`net8.0`)
- EF Core mapping to the existing PostgreSQL schema
- Route parity for finance/catalog/taxes/billing/admin/me endpoints
- Mock-header + basic-auth compatible authentication handler
- Error/validation response mapping compatible with existing API contracts
- Stripe-ready gateway for checkout sessions, payment intents, and webhook signature validation
- Noop fallback mode when Stripe secrets are not configured
- Strict-auth by default: requests without basic auth are rejected (`401`)
- Mock-header authentication is explicit opt-in via `Auth:EnableMockHeader=true` and only honored in non-production environments (`Development`/`Test`/`IntegrationTests`)
- Canonical permission namespace (`domain.action`) applied endpoint-by-endpoint
- Verification script: `acme-admin-dotnet/scripts/verify-batch3.sh`
- Embedded SQL migration runner (`Database/Migrations/*.sql`) with baseline mode for existing Java-managed schemas
- xUnit + Testcontainers integration suite: `acme-admin-dotnet/tests/Acme.Admin.Api.IntegrationTests`
- GitHub Actions integration workflow: `.github/workflows/dotnet-integration.yml`
- Legacy security permission alias migration (`*_READ`/`*_WRITE` -> canonical `domain.action`)
- Canonical permission normalization for legacy seeded roles (`ADMIN`, `MANAGER`, `VIEWER`)
- SQL migration `V006__canonical_security_permissions.sql` for DB-level legacy permission/role canonicalization
- SQL migration `V007__legacy_notnull_relaxation.sql` to unblock .NET create flows from old schema constraints
- Legacy runtime config alias removal: canonical .NET configuration keys only (`ConnectionStrings__Default`, `Stripe__*`, `Billing__Stripe__*`, `Auth__EnableMockHeader`)
- List endpoint pagination contract normalization (`page`/`size` + `items/page/size/total` response shape)
- Required-field parity hardening for create DTOs: missing value-type fields now fail request validation (`422`) instead of defaulting silently
- Policy hardening: canonical permission claims are now strictly required (no role-name bypass in authorization policies)
- Authentication hardening: mock/basic-auth permission claims are filtered to canonical permission codes only
- Role/write hardening: non-canonical permission codes are rejected for role upsert flows even if legacy rows still exist in DB
- Admin/profile response hardening: role and user permission lists are filtered to canonical codes only
- Legacy role template hardening: runtime seeding now canonicalizes `ADMIN`/`MANAGER`/`VIEWER` permission sets endpoint-safe
- Billing events hardening: pagination now follows the shared paging contract (no hidden size clamp) and malformed datetime-local filters return `400`
- Authentication hardening: removed deprecated default-user fallback authentication path
- Configuration hardening: empty/missing `ConnectionStrings:Default` now fails fast at startup
- Authentication hardening: mock-header auth is disabled by default and blocked in production even when configured
- Webhook hardening: Stripe webhook payload now requires `id` and `type` fields after signature verification
- Authentication hardening: when both Basic and mock headers are present, Basic auth takes precedence
- Tooling hardening: dashboard SSR proxy no longer injects mock headers by default (explicit opt-in only)
- Java backend history is preserved in the `backend-java-version` branch

## Run locally

```bash
cd acme-admin-dotnet
./start.sh
```

Or with Docker from repository root:

```bash
docker compose up -d --build
```

## CI test command

```bash
dotnet test acme-admin-dotnet/tests/Acme.Admin.Api.IntegrationTests/Acme.Admin.Api.IntegrationTests.csproj
```
