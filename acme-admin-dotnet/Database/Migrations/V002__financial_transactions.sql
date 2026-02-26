-- =============================================================================
-- V002: Financial Transactions - Orders, Bills, Expenses, Revenues, etc.
-- =============================================================================

-- ----------------------------------------
-- Orders
-- ----------------------------------------

CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(64) NOT NULL UNIQUE,
    issued_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    occurred_at TIMESTAMPTZ NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    total NUMERIC(15,2) NOT NULL,
    tax_ids JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ----------------------------------------
-- Bills (Accounts Payable)
-- ----------------------------------------

CREATE TABLE bills (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(64) NOT NULL UNIQUE,
    occurred_at TIMESTAMPTZ NOT NULL,
    due_at TIMESTAMPTZ,
    vendor VARCHAR(180),
    payee VARCHAR(180),
    total NUMERIC(15,2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    tax_ids JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ----------------------------------------
-- Expenses
-- ----------------------------------------

CREATE TABLE expenses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(64) NOT NULL UNIQUE,
    occurred_at TIMESTAMPTZ NOT NULL,
    description TEXT,
    subject VARCHAR(20) CHECK (subject IN ('OPERATIONAL', 'ADMINISTRATIVE', 'FINANCIAL', 'OTHER')),
    category_id UUID REFERENCES expense_categories(id),
    total NUMERIC(15,2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    tax_ids JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ----------------------------------------
-- Revenues
-- ----------------------------------------

CREATE TABLE revenues (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(64) NOT NULL UNIQUE,
    occurred_at TIMESTAMPTZ NOT NULL,
    description TEXT,
    source VARCHAR(180),
    total NUMERIC(15,2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    tax_ids JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ----------------------------------------
-- Purchases
-- ----------------------------------------

CREATE TABLE purchases (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(64) NOT NULL UNIQUE,
    occurred_at TIMESTAMPTZ NOT NULL,
    vendor VARCHAR(180),
    total NUMERIC(15,2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    tax_ids JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ----------------------------------------
-- Hirings (HR-related expenses)
-- ----------------------------------------

CREATE TABLE hirings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(64) NOT NULL UNIQUE,
    occurred_at TIMESTAMPTZ NOT NULL,
    employee_name VARCHAR(180) NOT NULL,
    candidate_name VARCHAR(180),
    role VARCHAR(120) NOT NULL,
    start_at TIMESTAMPTZ NOT NULL,
    end_at TIMESTAMPTZ,
    gross_salary NUMERIC(15,2) NOT NULL,
    total NUMERIC(15,2),
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    tax_ids JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ----------------------------------------
-- Budgets
-- ----------------------------------------

CREATE TABLE budgets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(64) NOT NULL UNIQUE,
    name VARCHAR(180) NOT NULL,
    description TEXT,
    period_start DATE NOT NULL,
    period_end DATE NOT NULL,
    total_amount NUMERIC(15,2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ----------------------------------------
-- Indexes
-- ----------------------------------------

CREATE INDEX idx_orders_code ON orders(code);
CREATE INDEX idx_orders_occurred ON orders(occurred_at);
CREATE INDEX idx_bills_code ON bills(code);
CREATE INDEX idx_bills_due ON bills(due_at);
CREATE INDEX idx_expenses_code ON expenses(code);
CREATE INDEX idx_expenses_occurred ON expenses(occurred_at);
CREATE INDEX idx_revenues_code ON revenues(code);
CREATE INDEX idx_revenues_occurred ON revenues(occurred_at);
CREATE INDEX idx_purchases_code ON purchases(code);
CREATE INDEX idx_hirings_code ON hirings(code);
CREATE INDEX idx_budgets_code ON budgets(code);
