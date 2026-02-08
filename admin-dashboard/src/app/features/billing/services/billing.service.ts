import { Injectable } from '@angular/core';
import { ApiClientService } from '../../../core/api/api-client.service';
import { ApiRoutes } from '../../../core/api/api.routes';
import type {
  CheckoutSessionRequest,
  CheckoutSessionResponse,
} from '../interfaces/billing.interfaces';

@Injectable({ providedIn: 'root' })
export class BillingService {
  constructor(private readonly api: ApiClientService) {}

  createCheckoutSession(req: CheckoutSessionRequest) {
    return this.api.post<CheckoutSessionResponse, CheckoutSessionRequest>(
      ApiRoutes.billing.checkoutSession,
      req,
    );
  }
}
