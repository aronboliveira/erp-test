import type { ChartSeriesMapperPort } from './chart.interfaces';
import type { NgxLineSeries, NgxSeriesPoint } from './chart.types';
import type { RevenueDto, ExpenseDto, BudgetDto } from '../../../core/api/api-contract.types';

export class ChartSeriesMapper implements ChartSeriesMapperPort {
  toRevenueLine(rev: readonly RevenueDto[]): readonly NgxLineSeries[] {
    const series: { name: string; value: number }[] = [];

    for (const r of rev) {
      const label = r.occurredAt.slice(0, 10);
      const value = Number(r.amount);
      Number.isFinite(value) ? series.push({ name: label, value }) : void 0;
    }

    return Object.freeze([{ name: 'Revenue', series: Object.freeze(series) }]);
  }

  toExpenseBars(exp: readonly ExpenseDto[]): readonly NgxSeriesPoint[] {
    const agg: Record<string, number> = {};
    for (const e of exp) {
      const k = e.category || 'Uncategorized';
      const v = Number(e.amount);
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
    const planned = budgets.reduce((a, b) => a + (Number(b.plannedAmount) || 0), 0);
    const spent = expenses.reduce((a, e) => a + (Number(e.amount) || 0), 0);

    const safePlanned = Number.isFinite(planned) ? planned : 0;
    const safeSpent = Number.isFinite(spent) ? spent : 0;
    const remaining = Math.max(0, safePlanned - safeSpent);

    return Object.freeze([
      Object.freeze({ name: 'Spent', value: safeSpent }),
      Object.freeze({ name: 'Remaining', value: remaining }),
    ]);
  }
}
