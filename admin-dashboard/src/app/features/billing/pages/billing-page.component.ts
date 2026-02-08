import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

import { BillingService } from '../services/billing.service';
import { SwalLoaderService } from '../../../core/ui/swal-loader.service';
import { DarkModeClassDirective } from '../../../shared/directives/dark-mode-class.directive';
import type { CheckoutSessionRequest } from '../interfaces/billing.interfaces';

type PlanId = 'starter' | 'pro';

const Plans = Object.freeze({
  starter: Object.freeze({ name: 'Starter', cents: 1990 }),
  pro: Object.freeze({ name: 'Pro', cents: 4990 }),
} as const);

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, DarkModeClassDirective],
  selector: 'app-billing-page',
  templateUrl: './billing-page.component.html',
  styleUrl: './billing-page.component.scss',
})
export class BillingPageComponent {
  plan: PlanId = 'pro';
  email: string = '';
  isProcessing = false;

  constructor(
    private readonly billing: BillingService,
    private readonly swal: SwalLoaderService,
    @Inject(PLATFORM_ID) private readonly platformId: object,
  ) {}

  async checkout(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    if (!this.emailValid) return;

    const Swal = (await this.swal.load()).default;

    const plan = Plans[this.plan];
    const ok = await Swal.fire({
      icon: 'question',
      title: 'Confirm purchase',
      html: `<div><strong>${plan.name}</strong> — ${(plan.cents / 100).toFixed(2)}</div>`,
      showCancelButton: true,
      confirmButtonText: 'Continue to payment',
      cancelButtonText: 'Cancel',
      focusConfirm: true,
    });

    if (!ok.isConfirmed) return;

    const req: CheckoutSessionRequest = {
      currency: 'brl',
      customerEmail: this.email?.trim() ? this.email.trim() : null,
      items: [{ name: `${plan.name} Plan`, unitAmountCents: plan.cents, quantity: 1 }],
    };

    let res: any = null;
    this.isProcessing = true;
    try {
      res = await firstValueFrom(this.billing.createCheckoutSession(req));
    } catch (e) {
      await Swal.fire({
        icon: 'error',
        title: 'Billing',
        text: 'Failed to create checkout session',
      });
      this.isProcessing = false;
      return;
    }
    this.isProcessing = false;

    const url = typeof res?.url === 'string' ? res.url : '';
    if (!url) {
      await Swal.fire({ icon: 'error', title: 'Billing', text: 'Invalid checkout URL' });
      return;
    }

    if (!this.#isTrustedCheckoutUrl(url)) {
      await Swal.fire({
        icon: 'error',
        title: 'Billing',
        text: 'Blocked an untrusted checkout URL',
      });
      return;
    }

    window.location.assign(url);
  }

  get emailValid(): boolean {
    if (!this.email) return true;
    const v = this.email.trim();
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);
  }

  #isTrustedCheckoutUrl(rawUrl: string): boolean {
    try {
      const url = new URL(rawUrl);
      const sameOrigin = url.origin === window.location.origin;
      if (sameOrigin) return true;

      const isHttps = url.protocol === 'https:';
      const allowedHosts = new Set(['checkout.stripe.com']);
      return isHttps && allowedHosts.has(url.host);
    } catch {
      return false;
    }
  }
}
