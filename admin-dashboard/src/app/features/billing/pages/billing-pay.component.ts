import { CommonModule } from '@angular/common';
import { Component, ElementRef, Inject, PLATFORM_ID, ViewChild } from '@angular/core';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { isPlatformBrowser } from '@angular/common';
import { firstValueFrom } from 'rxjs';

import { StripeLoaderService } from '../../../core/ui/stripe-loader.service';
import { ApiClientService } from '../../../core/api/api-client.service';
import { DomAttrGuard } from '../../../core/dom/dom-attr-guard';
import { RetryPolicy } from '../../../core/ui/retry-policy';
import { DarkModeClassDirective } from '../../../shared/directives/dark-mode-class.directive';

type PayForm = {
  amountCents: FormControl<number>;
  currency: FormControl<string>;
  email: FormControl<string | null>;
};

type PaymentIntentResponse = {
  provider: string;
  publishableKey: string;
  paymentIntentId: string;
  clientSecret: string;
  status: string;
};

@Component({
  standalone: true,
  selector: 'app-billing-pay',
  imports: [CommonModule, ReactiveFormsModule, DarkModeClassDirective],
  templateUrl: './billing-pay.component.html',
  styleUrl: './billing-pay.component.scss',
})
export class BillingPayComponent {
  @ViewChild('cardHost', { static: false }) cardHost?: ElementRef<HTMLDivElement>;

  isMounting = false;
  isConfirming = false;
  errorMessage = '';

  readonly form = new FormGroup<PayForm>({
    amountCents: new FormControl<number>(1990, {
      nonNullable: true,
      validators: [Validators.min(1)],
    }),
    currency: new FormControl<string>('brl', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/^[a-zA-Z]{3}$/)],
    }),
    email: new FormControl<string | null>(null, [Validators.email]),
  });

  constructor(
    private readonly api: ApiClientService,
    private readonly stripeLoader: StripeLoaderService,
    @Inject(PLATFORM_ID) private readonly platformId: object,
  ) {}

  async mountCard(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    if (this.isMounting || this.isConfirming) return;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const host = this.cardHost?.nativeElement;
    if (!host) return;
    const intentKey = this.#intentKey();
    const mountedKey = DomAttrGuard.get(host, 'stripe_card_mounted');
    if (mountedKey === intentKey) return;

    this.errorMessage = '';
    this.isMounting = true;
    try {
      const res = await this.#getOrCreateIntent();
      if (!res) return;

      await this.#ensureStripe(res.publishableKey, res.clientSecret, intentKey);
      if (!this.#elements || !this.#stripe) {
        this.errorMessage = 'Unable to initialize payment form. Please try again.';
        return;
      }

      host.replaceChildren();
      const card = this.#elements.create('payment');
      card.mount(host);
      DomAttrGuard.set(host, 'stripe_card_mounted', intentKey);
    } catch (e) {
      console.error('Failed to mount Stripe card', e);
      this.errorMessage = 'Unable to load the card element. Please try again.';
    } finally {
      this.isMounting = false;
    }
  }

  async confirm(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    if (this.isMounting || this.isConfirming) return;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const host = this.cardHost?.nativeElement;
    if (!host) return;
    await this.mountCard();

    this.errorMessage = '';
    this.isConfirming = true;
    try {
      const res = await this.#getOrCreateIntent();
      if (!res) return;
      if (!this.#elements || !this.#stripe) {
        this.errorMessage = 'Unable to initialize payment confirmation.';
        return;
      }

      await this.#stripe.confirmPayment({
        elements: this.#elements,
        confirmParams: {
          return_url: `${window.location.origin}/billing/success`,
        },
      });
    } catch (e) {
      console.error('Failed to confirm payment', e);
      this.errorMessage = 'Payment confirmation failed. Please try again.';
    } finally {
      this.isConfirming = false;
    }
  }

  #intentKey(): string {
    const v = this.form.getRawValue();
    const currency = String(v.currency || '').trim().toLowerCase();
    const email = v.email ? String(v.email).trim().toLowerCase() : '';
    return `${currency}:${v.amountCents}:${email}`;
  }

  async #getOrCreateIntent(): Promise<PaymentIntentResponse | null> {
    const key = this.#intentKey();
    if (this.#cachedIntent && this.#cachedIntentKey === key) return this.#cachedIntent;

    const v = this.form.getRawValue();
    const payload = {
      currency: String(v.currency || '').trim().toLowerCase(),
      amountCents: v.amountCents,
      customerEmail: v.email ? String(v.email).trim() : null,
      description: 'Admin dashboard test charge',
    };

    const res = await firstValueFrom(
      this.api.post<PaymentIntentResponse, any>('/api/billing/payment-intents', payload),
    );

    if (!res?.clientSecret || !res?.publishableKey) {
      this.errorMessage = 'Payment setup failed. Please try again.';
      return null;
    }

    this.#cachedIntent = res;
    this.#cachedIntentKey = key;
    return res;
  }

  async #ensureStripe(publishableKey: string, clientSecret: string, intentKey: string) {
    if (!this.#stripe || this.#publishableKey !== publishableKey) {
      this.#stripe = await this.stripeLoader.createStripe(publishableKey);
      this.#publishableKey = publishableKey;
    }

    if (!this.#elements || this.#mountedIntentKey !== intentKey) {
      this.#elements = this.#stripe.elements({ clientSecret });
      this.#mountedIntentKey = intentKey;
    }
  }

  #cachedIntentKey: string | null = null;
  #cachedIntent: PaymentIntentResponse | null = null;
  #stripe: any = null;
  #elements: any = null;
  #publishableKey: string | null = null;
  #mountedIntentKey: string | null = null;
}
