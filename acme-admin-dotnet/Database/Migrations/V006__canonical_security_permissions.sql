-- =============================================================================
-- V006: Canonical security permissions and legacy role migration
-- =============================================================================

-- Ensure canonical permission catalog exists.
insert into auth_permissions (id, code, description)
values
  (gen_random_uuid(), 'users.read', 'List/read users'),
  (gen_random_uuid(), 'users.write', 'Create/update users'),
  (gen_random_uuid(), 'roles.read', 'List/read roles'),
  (gen_random_uuid(), 'roles.write', 'Create/update roles'),
  (gen_random_uuid(), 'taxes.read', 'List/read taxes'),
  (gen_random_uuid(), 'taxes.write', 'Create/update/delete taxes'),
  (gen_random_uuid(), 'orders.read', 'List/read orders'),
  (gen_random_uuid(), 'orders.write', 'Create/update orders'),
  (gen_random_uuid(), 'procurement.read', 'List/read procurement data'),
  (gen_random_uuid(), 'procurement.write', 'Create/update procurement data'),
  (gen_random_uuid(), 'finance.read', 'Read finance data'),
  (gen_random_uuid(), 'finance.write', 'Write finance data'),
  (gen_random_uuid(), 'hr.read', 'Read HR data'),
  (gen_random_uuid(), 'hr.write', 'Write HR data'),
  (gen_random_uuid(), 'catalog.read', 'Read catalog'),
  (gen_random_uuid(), 'catalog.write', 'Write catalog'),
  (gen_random_uuid(), 'billing.read', 'Read billing data'),
  (gen_random_uuid(), 'billing.create', 'Create billing sessions'),
  (gen_random_uuid(), 'billing.pay', 'Pay billing intents')
on conflict (code) do nothing;

-- Map legacy permission grants from V004 seed to canonical policies.
with legacy_map (legacy_code, canonical_code) as (
  values
    ('DASHBOARD_READ', 'finance.read'),
    ('ORDERS_READ', 'orders.read'),
    ('ORDERS_WRITE', 'orders.write'),
    ('BILLS_READ', 'finance.read'),
    ('BILLS_WRITE', 'finance.write'),
    ('EXPENSES_READ', 'finance.read'),
    ('EXPENSES_WRITE', 'finance.write'),
    ('REVENUES_READ', 'finance.read'),
    ('REVENUES_WRITE', 'finance.write'),
    ('PRODUCTS_READ', 'catalog.read'),
    ('PRODUCTS_WRITE', 'catalog.write'),
    ('TAXES_READ', 'taxes.read'),
    ('TAXES_WRITE', 'taxes.write'),
    ('USERS_READ', 'users.read'),
    ('USERS_WRITE', 'users.write'),
    ('ROLES_READ', 'roles.read'),
    ('ROLES_WRITE', 'roles.write'),
    ('SETTINGS_READ', 'billing.read'),
    ('SETTINGS_WRITE', 'billing.create'),
    ('SETTINGS_WRITE', 'billing.pay')
)
insert into auth_role_permissions (role_id, permission_id)
select distinct arp.role_id, p_new.id
from auth_role_permissions arp
join auth_permissions p_old on p_old.id = arp.permission_id
join legacy_map lm on lm.legacy_code = p_old.code
join auth_permissions p_new on p_new.code = lm.canonical_code
on conflict do nothing;

-- Remove legacy permission grants and permission rows.
delete from auth_role_permissions arp
using auth_permissions p
where arp.permission_id = p.id
  and p.code in (
    'DASHBOARD_READ',
    'ORDERS_READ', 'ORDERS_WRITE',
    'BILLS_READ', 'BILLS_WRITE',
    'EXPENSES_READ', 'EXPENSES_WRITE',
    'REVENUES_READ', 'REVENUES_WRITE',
    'PRODUCTS_READ', 'PRODUCTS_WRITE',
    'TAXES_READ', 'TAXES_WRITE',
    'USERS_READ', 'USERS_WRITE',
    'ROLES_READ', 'ROLES_WRITE',
    'SETTINGS_READ', 'SETTINGS_WRITE'
  );

delete from auth_permissions
where code in (
  'DASHBOARD_READ',
  'ORDERS_READ', 'ORDERS_WRITE',
  'BILLS_READ', 'BILLS_WRITE',
  'EXPENSES_READ', 'EXPENSES_WRITE',
  'REVENUES_READ', 'REVENUES_WRITE',
  'PRODUCTS_READ', 'PRODUCTS_WRITE',
  'TAXES_READ', 'TAXES_WRITE',
  'USERS_READ', 'USERS_WRITE',
  'ROLES_READ', 'ROLES_WRITE',
  'SETTINGS_READ', 'SETTINGS_WRITE'
);

-- Ensure legacy role names still used by seeded/demo clients exist.
insert into auth_roles (id, name, description)
values
  (gen_random_uuid(), 'ADMIN', 'Full system administrator with canonical permissions'),
  (gen_random_uuid(), 'MANAGER', 'Manager with read/write access to business data'),
  (gen_random_uuid(), 'VIEWER', 'Read-only access to business data')
on conflict (name) do nothing;

update auth_roles set description = 'Full system administrator with canonical permissions'
where name = 'ADMIN' and (description is null or btrim(description) = '');

update auth_roles set description = 'Manager with read/write access to business data'
where name = 'MANAGER' and (description is null or btrim(description) = '');

update auth_roles set description = 'Read-only access to business data'
where name = 'VIEWER' and (description is null or btrim(description) = '');

-- Canonical role templates.
with role_perm (role_name, permission_code) as (
  values
    ('ADMIN', 'users.read'),
    ('ADMIN', 'users.write'),
    ('ADMIN', 'roles.read'),
    ('ADMIN', 'roles.write'),
    ('ADMIN', 'taxes.read'),
    ('ADMIN', 'taxes.write'),
    ('ADMIN', 'orders.read'),
    ('ADMIN', 'orders.write'),
    ('ADMIN', 'procurement.read'),
    ('ADMIN', 'procurement.write'),
    ('ADMIN', 'finance.read'),
    ('ADMIN', 'finance.write'),
    ('ADMIN', 'hr.read'),
    ('ADMIN', 'hr.write'),
    ('ADMIN', 'catalog.read'),
    ('ADMIN', 'catalog.write'),
    ('ADMIN', 'billing.read'),
    ('ADMIN', 'billing.create'),
    ('ADMIN', 'billing.pay'),

    ('MANAGER', 'catalog.read'),
    ('MANAGER', 'catalog.write'),
    ('MANAGER', 'taxes.read'),
    ('MANAGER', 'taxes.write'),
    ('MANAGER', 'orders.read'),
    ('MANAGER', 'orders.write'),
    ('MANAGER', 'procurement.read'),
    ('MANAGER', 'procurement.write'),
    ('MANAGER', 'finance.read'),
    ('MANAGER', 'finance.write'),
    ('MANAGER', 'hr.read'),
    ('MANAGER', 'hr.write'),
    ('MANAGER', 'billing.read'),
    ('MANAGER', 'billing.create'),
    ('MANAGER', 'billing.pay'),

    ('VIEWER', 'catalog.read'),
    ('VIEWER', 'taxes.read'),
    ('VIEWER', 'orders.read'),
    ('VIEWER', 'procurement.read'),
    ('VIEWER', 'finance.read'),
    ('VIEWER', 'hr.read'),
    ('VIEWER', 'billing.read'),
    ('VIEWER', 'users.read'),
    ('VIEWER', 'roles.read')
)
insert into auth_role_permissions (role_id, permission_id)
select r.id, p.id
from role_perm rp
join auth_roles r on r.name = rp.role_name
join auth_permissions p on p.code = rp.permission_code
on conflict do nothing;
