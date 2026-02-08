import type { ApiRouteRuntimeSpec } from './api-contract.interfaces';

export const API_ROUTES = Object.freeze({
  'catalog.list': { method: 'GET', path: '/api/catalog/items' },
  'catalog.create': { method: 'POST', path: '/api/catalog/items' },

  'finance.revenue.list': { method: 'GET', path: '/api/finance/revenue' },
  'finance.revenue.create': { method: 'POST', path: '/api/finance/revenue' },

  'finance.budget.list': { method: 'GET', path: '/api/finance/budgets' },
  'finance.budget.create': { method: 'POST', path: '/api/finance/budgets' },

  'finance.expense.list': { method: 'GET', path: '/api/finance/expenses' },
  'finance.expense.create': { method: 'POST', path: '/api/finance/expenses' },
} satisfies Record<string, ApiRouteRuntimeSpec>);
