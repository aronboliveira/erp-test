import { Component, Inject, PLATFORM_ID, signal, computed } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { NgxChartsModule } from '@swimlane/ngx-charts';

import { DarkModeClassDirective } from '../../shared/directives/dark-mode-class.directive';

type MoneyRow = Readonly<{
  date: string;
  category: string;
  ref: string;
  amount: number;
}>;

@Component({
  selector: 'app-expenses-page',
  standalone: true,
  imports: [CommonModule, NgxChartsModule, DarkModeClassDirective],
  templateUrl: './expenses-page.component.html',
  styleUrl: './expenses-page.component.scss',
})
export class ExpensesPageComponent {
  readonly #isBrowser: boolean;

  readonly filter = signal('');
  readonly filteredRows = computed(() => {
    const q = (this.filter() ?? '').trim().toLowerCase();
    return !q
      ? this.rows
      : this.rows.filter(
          (r) =>
            r.ref.toLowerCase().includes(q) ||
            r.category.toLowerCase().includes(q) ||
            r.date.includes(q),
        );
  });

  readonly totals = Object.freeze({
    month: 96420.1,
    today: 2870.0,
    pending: 4230.9,
  });

  readonly byCategory = Object.freeze([
    { name: 'Tax', value: 21340 },
    { name: 'Purchases', value: 39200 },
    { name: 'Hiring', value: 12400 },
    { name: 'Bills', value: 23680 },
  ]);

  readonly rows: readonly MoneyRow[] = Object.freeze([
    { date: '2026-01-29', category: 'Tax', ref: 'TAX-0021', amount: 820.2 },
    { date: '2026-01-28', category: 'Purchases', ref: 'PO-448', amount: 6490.0 },
    { date: '2026-01-27', category: 'Bills', ref: 'BILL-883', amount: 1200.0 },
    { date: '2026-01-26', category: 'Hiring', ref: 'HIR-014', amount: 3100.0 },
  ]);

  constructor(@Inject(PLATFORM_ID) platformId: object) {
    this.#isBrowser = isPlatformBrowser(platformId);
  }

  canRenderCharts(): boolean {
    return this.#isBrowser;
  }

  trackRow = (_: number, r: MoneyRow) => `${r.date}:${r.ref}`;

  fmtMoney(v: number): string {
    try {
      return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'BRL' }).format(v);
    } catch (_) {
      return `R$ ${v.toFixed(2)}`;
    }
  }
}
