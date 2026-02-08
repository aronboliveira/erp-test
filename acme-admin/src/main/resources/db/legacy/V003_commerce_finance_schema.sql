create extension if not exists pgcrypto;

create table if not exists product_or_service_categories (
  id uuid primary key,
  code text not null unique,
  title text not null,
  created_at timestamptz not null default now()
);

create table if not exists product_or_services (
  id uuid primary key,
  category_id uuid not null references product_or_service_categories(id),
  kind text not null,
  sku text not null unique,
  title text not null,
  unit_price_cents bigint not null,
  active boolean not null default true,
  created_at timestamptz not null default now(),
  constraint chk_pos_kind check (kind in ('PRODUCT','SERVICE'))
);

create table if not exists budgets (
  id uuid primary key,
  title text not null,
  period_start date not null,
  period_end date not null,
  currency text not null default 'brl',
  target_revenue_cents bigint not null default 0,
  max_expense_cents bigint not null default 0,
  created_at timestamptz not null default now(),
  constraint chk_budget_period check (period_end >= period_start)
);

create table if not exists revenues (
  id uuid primary key,
  occurred_at timestamptz not null,
  source text not null default 'manual',
  amount_cents bigint not null,
  currency text not null default 'brl',
  note text null,
  created_at timestamptz not null default now()
);

create table if not exists expense_categories (
  id uuid primary key,
  code text not null unique,
  title text not null,
  created_at timestamptz not null default now()
);

create table if not exists expenses (
  id uuid primary key,
  category_id uuid not null references expense_categories(id),
  occurred_at timestamptz not null,
  amount_cents bigint not null,
  currency text not null default 'brl',
  note text null,
  source_type text null,      -- OCP-friendly (Order, Purchase, Hiring, Bill, Invoice, ...)
  source_id uuid null,
  created_at timestamptz not null default now()
);

create table if not exists taxes (
  id uuid primary key,
  code text not null unique,
  title text not null,
  rate_bps int not null default 0, -- basis points (e.g., 250 = 2.5%)
  kind text not null default 'PERCENT',
  created_at timestamptz not null default now(),
  constraint chk_tax_kind check (kind in ('PERCENT','FIXED'))
);

create table if not exists orders (
  id uuid primary key,
  code text not null unique,
  occurred_at timestamptz not null,
  gross_cents bigint not null,
  currency text not null default 'brl',
  tax_ids jsonb null,
  created_at timestamptz not null default now(),
  constraint chk_orders_tax_ids_array check (tax_ids is null or jsonb_typeof(tax_ids) = 'array')
);

create table if not exists purchases (
  id uuid primary key,
  code text not null unique,
  occurred_at timestamptz not null,
  gross_cents bigint not null,
  currency text not null default 'brl',
  tax_ids jsonb null,
  created_at timestamptz not null default now(),
  constraint chk_purchases_tax_ids_array check (tax_ids is null or jsonb_typeof(tax_ids) = 'array')
);

create table if not exists hirings (
  id uuid primary key,
  code text not null unique,
  occurred_at timestamptz not null,
  gross_cents bigint not null,
  currency text not null default 'brl',
  tax_ids jsonb null,
  created_at timestamptz not null default now(),
  constraint chk_hirings_tax_ids_array check (tax_ids is null or jsonb_typeof(tax_ids) = 'array')
);

create table if not exists bills (
  id uuid primary key,
  code text not null unique,
  occurred_at timestamptz not null,
  gross_cents bigint not null,
  currency text not null default 'brl',
  tax_ids jsonb null,
  created_at timestamptz not null default now(),
  constraint chk_bills_tax_ids_array check (tax_ids is null or jsonb_typeof(tax_ids) = 'array')
);

create table if not exists invoices (
  id uuid primary key,
  code text not null unique,
  occurred_at timestamptz not null,
  gross_cents bigint not null,
  currency text not null default 'brl',
  created_at timestamptz not null default now()
);

create index if not exists idx_revenues_occurred_at on revenues(occurred_at);
create index if not exists idx_expenses_occurred_at on expenses(occurred_at);
create index if not exists idx_expenses_source on expenses(source_type, source_id);
create index if not exists idx_orders_occurred_at on orders(occurred_at);
create index if not exists idx_purchases_occurred_at on purchases(occurred_at);
create index if not exists idx_hirings_occurred_at on hirings(occurred_at);
create index if not exists idx_bills_occurred_at on bills(occurred_at);
create index if not exists idx_taxes_code on taxes(code);
