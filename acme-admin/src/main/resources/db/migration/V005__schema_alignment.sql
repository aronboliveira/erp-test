-- =============================================================================
-- V005: Align schema with JPA entities (dev safety)
-- =============================================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Taxes
ALTER TABLE taxes
  ADD COLUMN IF NOT EXISTS enabled BOOLEAN NOT NULL DEFAULT TRUE;
ALTER TABLE taxes
  ALTER COLUMN name TYPE VARCHAR(180);
UPDATE taxes SET enabled = active WHERE enabled IS NULL AND active IS NOT NULL;

-- Product/Service categories
ALTER TABLE product_or_service_categories
  ALTER COLUMN name TYPE VARCHAR(180);

-- Expense categories
ALTER TABLE expense_categories
  ALTER COLUMN name TYPE VARCHAR(180);
ALTER TABLE expense_categories
  ADD COLUMN IF NOT EXISTS subject VARCHAR(32) NOT NULL DEFAULT 'ORDER';

-- Products or Services
ALTER TABLE products_or_services
  ADD COLUMN IF NOT EXISTS sku VARCHAR(64);
ALTER TABLE products_or_services
  ADD COLUMN IF NOT EXISTS price NUMERIC(15,2);
UPDATE products_or_services SET price = unit_price WHERE price IS NULL;
ALTER TABLE products_or_services
  ALTER COLUMN price SET NOT NULL;

-- Expenses
ALTER TABLE expenses
  ADD COLUMN IF NOT EXISTS amount NUMERIC(15,2);
UPDATE expenses SET amount = total WHERE amount IS NULL;
ALTER TABLE expenses
  ALTER COLUMN amount SET NOT NULL;
ALTER TABLE expenses
  ADD COLUMN IF NOT EXISTS vendor VARCHAR(180);

-- Revenues
ALTER TABLE revenues
  ADD COLUMN IF NOT EXISTS amount NUMERIC(15,2);
UPDATE revenues SET amount = total WHERE amount IS NULL;
ALTER TABLE revenues
  ALTER COLUMN amount SET NOT NULL;
ALTER TABLE revenues
  ADD COLUMN IF NOT EXISTS source_ref VARCHAR(180);
UPDATE revenues SET source_ref = source WHERE source_ref IS NULL;

-- Budgets
ALTER TABLE budgets
  ADD COLUMN IF NOT EXISTS planned_amount NUMERIC(15,2);
UPDATE budgets SET planned_amount = total_amount WHERE planned_amount IS NULL;
ALTER TABLE budgets
  ALTER COLUMN planned_amount SET NOT NULL;

-- Billing events
ALTER TABLE billing_events
  ADD COLUMN IF NOT EXISTS event_id VARCHAR(128);
UPDATE billing_events SET event_id = external_id WHERE event_id IS NULL AND external_id IS NOT NULL;
ALTER TABLE billing_events
  ALTER COLUMN event_id SET NOT NULL;
ALTER TABLE billing_events
  ALTER COLUMN provider TYPE VARCHAR(32);
ALTER TABLE billing_events
  ALTER COLUMN event_type TYPE VARCHAR(128);
ALTER TABLE billing_events
  ALTER COLUMN payload TYPE TEXT USING payload::text;
CREATE UNIQUE INDEX IF NOT EXISTS uq_billing_events_event_id ON billing_events(event_id);
