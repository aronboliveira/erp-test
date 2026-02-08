import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { DomAttrGuard } from '../dom/dom-attr-guard';
import { RetryPolicy } from './retry-policy';

type StripeJs = typeof import('@stripe/stripe-js');
type Stripe = import('@stripe/stripe-js').Stripe;

@Injectable({ providedIn: 'root' })
export class StripeLoaderService {
  static readonly _datasetKey = 'stripe_js_loaded';
  static _mod: StripeJs | null = null;

  constructor(@Inject(PLATFORM_ID) private readonly platformId: object) {}

  async load(policy: RetryPolicy = new RetryPolicy(6, 200)): Promise<StripeJs> {
    if (!isPlatformBrowser(this.platformId)) throw new Error('StripeLoader: browser required');
    if (StripeLoaderService._mod) return StripeLoaderService._mod;

    const root = document.documentElement;
    if (DomAttrGuard.has(root, StripeLoaderService._datasetKey) && StripeLoaderService._mod)
      return StripeLoaderService._mod;

    let last: unknown = null;
    for (let i = 0; i < policy.tries; i++) {
      try {
        const mod = await import('@stripe/stripe-js');
        StripeLoaderService._mod = mod;
        DomAttrGuard.set(root, StripeLoaderService._datasetKey, '1');
        return mod;
      } catch (e) {
        last = e;
        await new Promise<void>((r) => setTimeout(r, policy.intervalMs));
      }
    }
    throw last instanceof Error ? last : new Error('StripeLoader: failed');
  }

  async createStripe(publishableKey: string): Promise<Stripe> {
    const mod = await this.load();
    const stripe = await mod.loadStripe(publishableKey);
    if (!stripe) throw new Error('StripeLoader: loadStripe returned null');
    return stripe;
  }
}
