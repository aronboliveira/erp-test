import { Component } from '@angular/core';
import { DataTableComponent } from '../../shared/table/data-table.component';
import { DarkModeClassDirective } from '../../shared/directives/dark-mode-class.directive';
import type { ColumnDef } from '../../shared/table/data-table.types';

type OrderRow = Readonly<{
  id: string;
  customer: { name: string };
  total: number;
  status: 'pending' | 'paid' | 'shipped' | 'canceled';
}>;

@Component({
  standalone: true,
  selector: 'app-orders-page',
  imports: [DataTableComponent, DarkModeClassDirective],
  template: `
    <section appDarkModeClass class="page page--orders" aria-label="Orders">
      <header class="page__header">
        <div>
          <div class="page__titleRow">
            <span class="page__titleIcon" aria-hidden="true">
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="1.5"
                stroke-linecap="round"
                stroke-linejoin="round"
                focusable="false"
                aria-hidden="true"
              >
                <path
                  d="M2.25 3.75a.75.75 0 0 1 .75-.75h2.386a.75.75 0 0 1 .75.75v.99c0 .083.013.164.04.24l1.445 4.06a.75.75 0 0 0 .706.502h9.656a.75.75 0 0 0 .709-.49l1.234-3.592a.75.75 0 0 0-.709-.99H6.944"
                ></path>
                <path d="M16.5 21a1.5 1.5 0 1 0-3 0 1.5 1.5 0 0 0 3 0Z"></path>
                <path d="M8.25 21a1.5 1.5 0 1 0-3 0 1.5 1.5 0 0 0 3 0Z"></path>
              </svg>
            </span>
            <h1 class="page__title">Orders</h1>
          </div>
          <p class="page__subtitle">Latest purchases and fulfillment status.</p>
        </div>
      </header>

      <section class="panel" aria-label="Orders table">
        <div class="panel__head">
          <div>
            <div class="panel__title">Latest orders</div>
            <div class="panel__hint">Updated from placeholder data</div>
          </div>
        </div>
        <div class="panel__body">
          <app-data-table
            [columns]="columns"
            [rows]="rows"
            ariaLabel="Orders table"
            caption="Orders list"
          ></app-data-table>
        </div>
      </section>
    </section>
  `,
})
export class OrdersPageComponent {
  readonly columns: readonly ColumnDef<OrderRow>[] = [
    { id: 'id', header: 'Order ID', kind: 'path', path: 'id' },
    { id: 'customer', header: 'Customer', kind: 'path', path: 'customer.name' },
    { id: 'total', header: 'Total', kind: 'render', render: (r) => `$${r.total.toFixed(2)}` },
    { id: 'status', header: 'Status', kind: 'path', path: 'status' },
  ];

  readonly rows: readonly OrderRow[] = [
    { id: 'A-1001', customer: { name: 'Jane Doe' }, total: 120.5, status: 'paid' },
    { id: 'A-1002', customer: { name: 'John Smith' }, total: 89.99, status: 'pending' },
  ];
}
