import type { NgxLineSeries, NgxSeriesPoint } from './chart.types';
import type { RevenueDto, ExpenseDto, BudgetDto } from '../../../core/api/api-contract.types';

export interface ChartSeriesMapperPort {
  toRevenueLine(rev: readonly RevenueDto[]): readonly NgxLineSeries[];
  toExpenseBars(exp: readonly ExpenseDto[]): readonly NgxSeriesPoint[];
  toBudgetDonut(
    budgets: readonly BudgetDto[],
    expenses: readonly ExpenseDto[],
  ): readonly NgxSeriesPoint[];
}
