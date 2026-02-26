-- =============================================================================
-- V007: Relax legacy NOT NULL constraints not used by .NET entity model
-- =============================================================================

-- Catalog items: legacy code/unit_price were required in the old model.
alter table products_or_services
  alter column code drop not null;

alter table products_or_services
  alter column unit_price drop not null;

-- Expenses: legacy code/total were replaced by amount.
alter table expenses
  alter column code drop not null;

alter table expenses
  alter column total drop not null;

-- Revenues: legacy code/total were replaced by amount.
alter table revenues
  alter column code drop not null;

alter table revenues
  alter column total drop not null;

-- Budgets: legacy code/name/total_amount were replaced by planned_amount.
alter table budgets
  alter column code drop not null;

alter table budgets
  alter column name drop not null;

alter table budgets
  alter column total_amount drop not null;
