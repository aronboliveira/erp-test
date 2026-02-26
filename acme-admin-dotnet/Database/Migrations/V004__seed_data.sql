-- =============================================================================
-- V004: Seed Data - Default admin user and basic permissions
-- =============================================================================

-- ----------------------------------------
-- Default Permissions
-- ----------------------------------------

INSERT INTO auth_permissions (id, code, description) VALUES
    (gen_random_uuid(), 'DASHBOARD_READ', 'View Dashboard'),
    (gen_random_uuid(), 'ORDERS_READ', 'View Orders'),
    (gen_random_uuid(), 'ORDERS_WRITE', 'Create/Edit Orders'),
    (gen_random_uuid(), 'BILLS_READ', 'View Bills'),
    (gen_random_uuid(), 'BILLS_WRITE', 'Create/Edit Bills'),
    (gen_random_uuid(), 'EXPENSES_READ', 'View Expenses'),
    (gen_random_uuid(), 'EXPENSES_WRITE', 'Create/Edit Expenses'),
    (gen_random_uuid(), 'REVENUES_READ', 'View Revenues'),
    (gen_random_uuid(), 'REVENUES_WRITE', 'Create/Edit Revenues'),
    (gen_random_uuid(), 'PRODUCTS_READ', 'View Products'),
    (gen_random_uuid(), 'PRODUCTS_WRITE', 'Create/Edit Products'),
    (gen_random_uuid(), 'TAXES_READ', 'View Taxes'),
    (gen_random_uuid(), 'TAXES_WRITE', 'Create/Edit Taxes'),
    (gen_random_uuid(), 'USERS_READ', 'View Users'),
    (gen_random_uuid(), 'USERS_WRITE', 'Create/Edit Users'),
    (gen_random_uuid(), 'ROLES_READ', 'View Roles'),
    (gen_random_uuid(), 'ROLES_WRITE', 'Create/Edit Roles'),
    (gen_random_uuid(), 'SETTINGS_READ', 'View Settings'),
    (gen_random_uuid(), 'SETTINGS_WRITE', 'Modify Settings');

-- ----------------------------------------
-- Default Roles
-- ----------------------------------------

INSERT INTO auth_roles (id, name, description) VALUES
    ('00000000-0000-0000-0000-000000000001', 'ADMIN', 'Full system administrator with all permissions'),
    ('00000000-0000-0000-0000-000000000002', 'MANAGER', 'Manager with read/write access to business data'),
    ('00000000-0000-0000-0000-000000000003', 'VIEWER', 'Read-only access to dashboard and reports');

-- Assign all permissions to ADMIN role
INSERT INTO auth_role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0000-000000000001', id FROM auth_permissions;

-- Assign read permissions to VIEWER role  
INSERT INTO auth_role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0000-000000000003', id FROM auth_permissions WHERE code LIKE '%_READ';

-- ----------------------------------------
-- Default Admin User (password: admin123)
-- BCrypt hash of 'admin123'
-- ----------------------------------------

INSERT INTO auth_users (id, username, password_hash, email, display_name) VALUES
    ('00000000-0000-0000-0000-000000000001', 
     'admin', 
     '$2a$10$N9qo8uLOickgx2ZMRZoMye.IjqQBerkPCOcWlkG2sZp6E6bNpPMHi', 
     'admin@acme.local', 
     'System Administrator');

-- Assign ADMIN role to admin user
INSERT INTO auth_user_roles (user_id, role_id) VALUES
    ('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001');
