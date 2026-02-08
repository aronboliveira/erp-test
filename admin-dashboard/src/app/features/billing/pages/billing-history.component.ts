import { CommonModule } from '@angular/common';
import { Component, Inject, PLATFORM_ID, OnInit, OnDestroy } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup } from '@angular/forms';
import { isPlatformBrowser } from '@angular/common';
import { firstValueFrom, Subscription } from 'rxjs';

import { BillingHistoryService } from '../services/billing-history.service';
import { FormPersistService } from '../../../lib/shared/services/form-persist.service';
import { DateValidator } from '../../../core/dates/date-validator.adapter';
import { DarkModeClassDirective } from '../../../shared/directives/dark-mode-class.directive';

type FilterForm = {
  provider: FormControl<string | null>;
  eventType: FormControl<string | null>;
  receivedFrom: FormControl<string | null>; // datetime-local
  receivedTo: FormControl<string | null>; // datetime-local
};

const FILTER_KEY = '_billing_events_filter';

@Component({
  standalone: true,
  selector: 'app-billing-history',
  imports: [CommonModule, ReactiveFormsModule, DarkModeClassDirective],
  templateUrl: './billing-history.component.html',
  styleUrl: './billing-history.component.scss',
})
export class BillingHistoryComponent implements OnInit, OnDestroy {
  rows: any[] = [];
  page = 0;
  size = 10;
  total = 0;
  isLoading = false;
  errorMessage = '';
  rangeInvalid = false;
  #persistSub: Subscription | null = null;
  #formSub: Subscription | null = null;

  readonly form = new FormGroup<FilterForm>({
    provider: new FormControl<string | null>(null),
    eventType: new FormControl<string | null>(null),
    receivedFrom: new FormControl<string | null>(null),
    receivedTo: new FormControl<string | null>(null),
  });

  constructor(
    private readonly svc: BillingHistoryService,
    private readonly persist: FormPersistService,
    @Inject(PLATFORM_ID) private readonly platformId: object,
  ) {}

  async ngOnInit(): Promise<void> {
    if (isPlatformBrowser(this.platformId)) {
      this.persist.restore(FILTER_KEY, this.form);
      this.#persistSub = this.persist.bind(FILTER_KEY, this.form);
    }
    this.#formSub = this.form.valueChanges.subscribe(() => {
      if (this.errorMessage) this.errorMessage = '';
      if (this.rangeInvalid) this.rangeInvalid = false;
    });
    await this.load(0);
  }

  ngOnDestroy(): void {
    this.#persistSub?.unsubscribe();
    this.#formSub?.unsubscribe();
  }

  async load(page: number): Promise<void> {
    if (this.isLoading) return;
    const v = this.form.getRawValue();
    const from =
      v.receivedFrom && DateValidator.classifyTemporal(v.receivedFrom) ? v.receivedFrom : null;
    const to = v.receivedTo && DateValidator.classifyTemporal(v.receivedTo) ? v.receivedTo : null;

    this.rangeInvalid = false;
    if (!this.#validateRange(from, to)) return;

    this.isLoading = true;
    this.errorMessage = '';
    try {
      const res = await firstValueFrom(
        this.svc.pageFiltered({
          page,
          size: this.size,
          provider: v.provider?.trim() || null,
          eventType: v.eventType?.trim() || null,
          receivedFrom: from,
          receivedTo: to,
        }),
      );

      this.page = res.page;
      this.size = res.size;
      this.total = res.total;

      this.rows = res.items.map((r) => ({
        ...r,
        prettyDate: this.prettyDatetimeLocal(r.receivedAt),
      }));
    } catch (e) {
      console.error('Failed to load billing history', e);
      this.errorMessage = 'Unable to load billing history. Please try again.';
    } finally {
      this.isLoading = false;
    }
  }

  async next(): Promise<void> {
    const maxPage = Math.max(0, Math.ceil(this.total / this.size) - 1);
    const n = this.page + 1;
    n <= maxPage ? await this.load(n) : null;
  }

  async prev(): Promise<void> {
    const p = this.page - 1;
    p >= 0 ? await this.load(p) : null;
  }

  trackRow = (_: number, r: { id?: string; eventId?: string }) => r.id || r.eventId || _;

  get isFirstPage(): boolean {
    return this.page <= 0;
  }

  get isLastPage(): boolean {
    const maxPage = Math.max(0, Math.ceil(this.total / this.size) - 1);
    return this.page >= maxPage;
  }

  prettyDatetimeLocal(v: string): string {
    const c = DateValidator.classifyTemporal(v);
    if (!c) return v;
    const d = DateValidator.toDate(c.kind, c.normalized);
    return `${d.toLocaleDateString()} ${d.toLocaleTimeString()}`;
  }

  #validateRange(from: string | null, to: string | null): boolean {
    if (!from || !to) return true;
    try {
      const fromMeta = DateValidator.classifyTemporal(from);
      const toMeta = DateValidator.classifyTemporal(to);
      if (!fromMeta || !toMeta) return true;
      const fromDate = DateValidator.toDate(fromMeta.kind, fromMeta.normalized);
      const toDate = DateValidator.toDate(toMeta.kind, toMeta.normalized);
      if (Number.isNaN(fromDate.getTime()) || Number.isNaN(toDate.getTime())) return true;
      if (fromDate.getTime() > toDate.getTime()) {
        this.rangeInvalid = true;
        this.errorMessage = 'Start date must be before end date.';
        return false;
      }
      return true;
    } catch (e) {
      console.error('Failed to validate date range', e);
      return true;
    }
  }
}
