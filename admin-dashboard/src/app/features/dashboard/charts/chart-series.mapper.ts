import type { ChartSeriesMapperPort } from './chart.interfaces';
import type { NgxLineSeries, NgxSeriesPoint } from './chart.types';
import type { RevenueDto, ExpenseDto, BudgetDto } from '../../../core/api/api-contract.types';

export class ChartSeriesMapper implements ChartSeriesMapperPort {
  toRevenueLine(rev: readonly RevenueDto[]): readonly NgxLineSeries[] {
    const series: { name: string; value: number }[] = [];

    for (const r of rev) {
      const label = r.occurredAt.slice(0, 10);
      const value = this.#readMoney(r, ['amount', 'total', 'amountCents']);
      Number.isFinite(value) ? series.push({ name: label, value }) : void 0;
    }

    return Object.freeze([{ name: 'Revenue', series: Object.freeze(series) }]);
  }

  toExpenseBars(exp: readonly ExpenseDto[]): readonly NgxSeriesPoint[] {
    const agg: Record<string, number> = {};
    for (const e of exp) {
      const anyExpense = e as unknown as Record<string, unknown>;
      const k = String(anyExpense['category'] ?? anyExpense['categoryId'] ?? 'Uncategorized');
      const v = this.#readMoney(e, ['amount', 'total', 'amountCents']);
      Number.isFinite(v) ? (agg[k] = (agg[k] || 0) + v) : void 0;
    }

    const out: NgxSeriesPoint[] = Object.keys(agg)
      .sort()
      .map((k) => Object.freeze({ name: k, value: agg[k] }));
    return Object.freeze(out);
  }

  toBudgetDonut(
    budgets: readonly BudgetDto[],
    expenses: readonly ExpenseDto[],
  ): readonly NgxSeriesPoint[] {
    const planned = budgets.reduce((a, b) => a + this.#readMoney(b, ['plannedAmount', 'totalAmount']), 0);
    const spent = expenses.reduce((a, e) => a + this.#readMoney(e, ['amount', 'total', 'amountCents']), 0);

    const safePlanned = Number.isFinite(planned) ? planned : 0;
    const safeSpent = Number.isFinite(spent) ? spent : 0;
    const remaining = Math.max(0, safePlanned - safeSpent);

    return Object.freeze([
      Object.freeze({ name: 'Spent', value: safeSpent }),
      Object.freeze({ name: 'Remaining', value: remaining }),
    ]);
  }

  #readMoney(obj: unknown, keys: readonly string[]): number {
    const record = obj as Record<string, unknown>;
    for (const key of keys) {
      const raw = record[key];
      if (raw === null || raw === undefined) continue;

      const parsed = Number(raw);
      if (!Number.isFinite(parsed)) continue;

      if (key.toLowerCase().includes('cents')) return parsed / 100;
      return parsed;
    }
    return 0;
  }
}
