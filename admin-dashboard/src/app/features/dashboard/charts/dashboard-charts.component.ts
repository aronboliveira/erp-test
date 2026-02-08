import { Component, inject } from '@angular/core';
import { NgxChartsModule } from '@swimlane/ngx-charts';

import type { ChartTileSpec } from './chart.types';
import { ChartSeriesMapper } from './chart-series.mapper';
import { ApiClientService } from '../../../core/api/api-client.service';

@Component({
  standalone: true,
  selector: 'app-dashboard-charts',
  imports: [NgxChartsModule],
  template: `
    <section class="panel-grid panel-grid--charts" aria-label="Dashboard charts">
      @for (t of tiles; track t.id) {
        <article class="panel chart-panel" [attr.aria-label]="t.ariaLabel">
          <header class="panel__head">
            <div>
              <div class="panel__title">{{ t.title }}</div>
              <div class="panel__hint">{{ t.ariaLabel }}</div>
            </div>
          </header>

          <div class="panel__body chart-panel__body" role="img" [attr.aria-label]="t.ariaLabel">
            @if (t.kind === 'revenueLine') {
              <ngx-charts-line-chart
                [results]="revenueLine"
                [legend]="false"
                [animations]="true"
                [xAxis]="true"
                [yAxis]="true"
                [timeline]="true"
              ></ngx-charts-line-chart>
            }

            @if (t.kind === 'expenseBars') {
              <ngx-charts-bar-vertical
                [results]="expenseBars"
                [legend]="false"
                [animations]="true"
                [xAxis]="true"
                [yAxis]="true"
              ></ngx-charts-bar-vertical>
            }

            @if (t.kind === 'budgetDonut') {
              <ngx-charts-pie-chart
                [results]="budgetDonut"
                [legend]="true"
                [animations]="true"
                [doughnut]="true"
              ></ngx-charts-pie-chart>
            }
          </div>
        </article>
      }
    </section>
  `,
})
export class DashboardChartsComponent {
  readonly #api = inject(ApiClientService);
  readonly #mapper = new ChartSeriesMapper();

  readonly tiles: readonly ChartTileSpec[] = Object.freeze([
    Object.freeze({
      id: 'rev',
      title: 'Revenue (trend)',
      kind: 'revenueLine',
      ariaLabel: 'Revenue line chart',
    }),
    Object.freeze({
      id: 'exp',
      title: 'Expenses (by category)',
      kind: 'expenseBars',
      ariaLabel: 'Expenses bar chart',
    }),
    Object.freeze({
      id: 'bud',
      title: 'Budget usage',
      kind: 'budgetDonut',
      ariaLabel: 'Budget donut chart',
    }),
  ]);

  revenueLine: any[] = [];
  expenseBars: any[] = [];
  budgetDonut: any[] = [];

  constructor() {
    void this.#load();
  }

  async #load(): Promise<void> {
    try {
      const [rev, exp, bud] = await Promise.all([
        this.#api.call('finance.revenue.list'),
        this.#api.call('finance.expense.list'),
        this.#api.call('finance.budget.list'),
      ]);

      this.revenueLine = this.#mapper.toRevenueLine(rev) as any;
      this.expenseBars = this.#mapper.toExpenseBars(exp) as any;
      this.budgetDonut = this.#mapper.toBudgetDonut(bud, exp) as any;
    } catch (e) {
      console.error('DashboardChartsComponent load failed', e);
    }
  }
}
