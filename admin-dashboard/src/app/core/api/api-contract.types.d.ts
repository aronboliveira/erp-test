export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export type RouteId =
  | 'catalog.list'
  | 'catalog.create'
  | 'finance.revenue.list'
  | 'finance.revenue.create'
  | 'finance.budget.list'
  | 'finance.budget.create'
  | 'finance.expense.list'
  | 'finance.expense.create';

export type ApiEnvelope<T> = Readonly<{ data: T }>;

export type ProductKind = 'PRODUCT' | 'SERVICE';

export type ProductOrServiceDto = Readonly<{
  id: string;
  kind: ProductKind;
  name: string;
  sku: string | null;
  price: string; // decimal-as-string
  currency: string;
}>;

export type RevenueDto = Readonly<{
  id: string;
  occurredAt: string; // ISO
  amount: string;
  currency: string;
  sourceRef: string | null;
}>;

export type BudgetDto = Readonly<{
  id: string;
  periodStart: string; // ISO date
  periodEnd: string; // ISO date
  plannedAmount: string;
  currency: string;
}>;

export type ExpenseDto = Readonly<{
  id: string;
  occurredAt: string; // ISO
  amount: string;
  currency: string;
  category: string;
  vendor: string | null;
}>;

export type ApiContract = Readonly<{
  'catalog.list': {
    method: 'GET';
    path: '/api/catalog/items';
    query?: Readonly<{ q?: string }>;
    response: readonly ProductOrServiceDto[];
  };
  'catalog.create': {
    method: 'POST';
    path: '/api/catalog/items';
    body: Readonly<Omit<ProductOrServiceDto, 'id'>>;
    response: ProductOrServiceDto;
  };

  'finance.revenue.list': {
    method: 'GET';
    path: '/api/finance/revenue';
    response: readonly RevenueDto[];
  };
  'finance.revenue.create': {
    method: 'POST';
    path: '/api/finance/revenue';
    body: Readonly<Omit<RevenueDto, 'id'>>;
    response: RevenueDto;
  };

  'finance.budget.list': {
    method: 'GET';
    path: '/api/finance/budgets';
    response: readonly BudgetDto[];
  };
  'finance.budget.create': {
    method: 'POST';
    path: '/api/finance/budgets';
    body: Readonly<Omit<BudgetDto, 'id'>>;
    response: BudgetDto;
  };

  'finance.expense.list': {
    method: 'GET';
    path: '/api/finance/expenses';
    response: readonly ExpenseDto[];
  };
  'finance.expense.create': {
    method: 'POST';
    path: '/api/finance/expenses';
    body: Readonly<Omit<ExpenseDto, 'id'>>;
    response: ExpenseDto;
  };
}>;

export type ReqOf<I extends RouteId> = ApiContract[I] extends { body: infer B } ? B : undefined;
export type QueryOf<I extends RouteId> = ApiContract[I] extends { query: infer Q } ? Q : undefined;
export type ResOf<I extends RouteId> = ApiContract[I]['response'];
export type ProductOrServiceCategoryDto = Readonly<{
  id: string;
  code: string;
  name: string;
  description: string | null;
}>;

export type ExpenseSubject = 'ORDER' | 'PURCHASE' | 'TAX' | 'HIRING' | 'BILL' | 'INVOICE';

export type ExpenseCategoryDto = Readonly<{
  id: string;
  code: string;
  name: string;
  subject: ExpenseSubject;
  description: string | null;
}>;

export type ProductOrServiceDto = Readonly<{
  id: string;
  kind: ProductKind;
  name: string;
  sku: string | null;
  price: string;
  currency: string;
  categoryId: string;
}>;

export type ExpenseDto = Readonly<{
  id: string;
  occurredAt: string;
  amount: string;
  currency: string;
  categoryId: string;
  vendor: string | null;
}>;

export type TaxDto = Readonly<{
  id: string;
  code: string;
  name: string;
  rate: string;
  enabled: boolean;
}>;

export type TaxIdList = readonly string[] | null;

export type OrderDto = Readonly<{
  id: string;
  code: string;
  occurredAt: string;
  currency: string;
  total: string;
  taxIds: TaxIdList;
}>;
