import { Injectable } from '@angular/core';
import { ApiClientService } from '../../../core/api/api-client.service';

export interface BillingEventRow {
  id: string;
  provider: string;
  eventId: string;
  eventType: string;
  receivedAt: string; // datetime-local string
}

export interface BillingEventPage {
  items: BillingEventRow[];
  page: number;
  size: number;
  total: number;
}

export interface BillingEventFilter {
  page: number;
  size: number;
  provider?: string | null;
  eventType?: string | null;
  receivedFrom?: string | null;
  receivedTo?: string | null;
}

@Injectable({ providedIn: 'root' })
export class BillingHistoryService {
  constructor(private readonly api: ApiClientService) {}

  pageFiltered(f: BillingEventFilter) {
    const qp = new URLSearchParams();
    qp.set('page', String(f.page ?? 0));
    qp.set('size', String(f.size ?? 10));
    f.provider ? qp.set('provider', f.provider) : null;
    f.eventType ? qp.set('eventType', f.eventType) : null;
    f.receivedFrom ? qp.set('receivedFrom', f.receivedFrom) : null;
    f.receivedTo ? qp.set('receivedTo', f.receivedTo) : null;

    return this.api.get<BillingEventPage>(`/api/billing/events?${qp.toString()}`);
  }

  page(page = 0, size = 10) {
    return this.api.get<BillingEventPage>(`/api/billing/events?page=${page}&size=${size}`);
  }
}
