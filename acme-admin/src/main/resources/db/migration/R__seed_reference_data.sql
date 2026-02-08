-- Product/Service categories
insert into product_or_service_categories (id, code, name, description)
values
  (gen_random_uuid(), 'PRODUCTS', 'Products', null),
  (gen_random_uuid(), 'SERVICES', 'Services', null),
  (gen_random_uuid(), 'SUBSCRIPTIONS', 'Subscriptions', null),
  (gen_random_uuid(), 'SUPPORT', 'Support & Maintenance', null)
on conflict (code) do nothing;

-- Expense categories (aligned with ExpenseSubject enum)
insert into expense_categories (id, code, name, subject, description)
values
  (gen_random_uuid(), 'ORDER', 'Order', 'ORDER', null),
  (gen_random_uuid(), 'PURCHASE', 'Purchase', 'PURCHASE', null),
  (gen_random_uuid(), 'TAX', 'Tax', 'TAX', null),
  (gen_random_uuid(), 'HIRING', 'Hiring', 'HIRING', null),
  (gen_random_uuid(), 'BILL', 'Bill', 'BILL', null),
  (gen_random_uuid(), 'INVOICE', 'Invoice', 'INVOICE', null)
on conflict (code) do nothing;

-- Taxes (rate as decimal, e.g. 0.02 = 2%)
insert into taxes (id, code, name, rate, enabled)
values
  (gen_random_uuid(), 'ISS_2', 'ISS 2%', 0.0200, true),
  (gen_random_uuid(), 'ISS_5', 'ISS 5%', 0.0500, true),
  (gen_random_uuid(), 'ICMS_12', 'ICMS 12%', 0.1200, true),
  (gen_random_uuid(), 'SERVICE_FEE_1_5', 'Service Fee 1.5%', 0.0150, true)
on conflict (code) do nothing;
