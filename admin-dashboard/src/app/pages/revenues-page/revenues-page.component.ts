import { Component, Inject, PLATFORM_ID, signal, computed } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { NgxChartsModule } from '@swimlane/ngx-charts';

import { DarkModeClassDirective } from '../../shared/directives/dark-mode-class.directive';

type MoneyRow = Readonly<{
  date: string;
  source: string;
  amount: number;
}>;

@Component({
  selector: 'app-revenues-page',
  standalone: true,
  imports: [CommonModule, NgxChartsModule, DarkModeClassDirective],
  templateUrl: './revenues-page.component.html',
  styleUrl: './revenues-page.component.scss',
})
export class RevenuesPageComponent {
  readonly #isBrowser: boolean;

  readonly filter = signal('');
  readonly filteredRows = computed(() => {
    const q = (this.filter() ?? '').trim().toLowerCase();
    return !q
      ? this.rows
      : this.rows.filter((r) => r.source.toLowerCase().includes(q) || r.date.includes(q));
  });

  readonly totals = Object.freeze({
    month: 182340.55,
    today: 7340.2,
    pending: 15690.1,
  });

  readonly line = Object.freeze([
    {
      name: 'Revenue',
      series: [
        { name: '2026-01-01', value: 8200 },
        { name: '2026-01-08', value: 12100 },
        { name: '2026-01-15', value: 9800 },
        { name: '2026-01-22', value: 15300 },
        { name: '2026-01-29', value: 14700 },
      ],
    },
  ]);

  readonly byChannel = Object.freeze([
    { name: 'Storefront', value: 102400 },
    { name: 'Marketplace', value: 52300 },
    { name: 'Subscriptions', value: 27640 },
  ]);

  readonly rows: readonly MoneyRow[] = Object.freeze([
    { date: '2026-01-29', source: 'Order #A-1182', amount: 1290.5 },
    { date: '2026-01-29', source: 'Order #A-1181', amount: 870.0 },
    { date: '2026-01-28', source: 'Invoice #INV-901', amount: 4500.0 },
    { date: '2026-01-27', source: 'Order #A-1175', amount: 320.4 },
    { date: '2026-01-26', source: 'Subscription', amount: 99.9 },
  ]);

  constructor(@Inject(PLATFORM_ID) platformId: object) {
    this.#isBrowser = isPlatformBrowser(platformId);
  }

  canRenderCharts(): boolean {
    return this.#isBrowser;
  }

  trackRow = (_: number, r: MoneyRow) => `${r.date}:${r.source}`;

  fmtMoney(v: number): string {
    try {
      return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'BRL' }).format(v);
    } catch (_) {
      return `R$ ${v.toFixed(2)}`;
    }
  }
}
