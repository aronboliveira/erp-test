import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { DomAttrGuard } from '../dom/dom-attr-guard';
import { RetryPolicy } from './retry-policy';

type SwalModule = typeof import('sweetalert2');

@Injectable({ providedIn: 'root' })
export class SwalLoaderService {
  static readonly _datasetKey = 'swal_loaded';
  static _cached: SwalModule | null = null;

  constructor(@Inject(PLATFORM_ID) private readonly platformId: object) {}

  async load(policy: RetryPolicy = new RetryPolicy(6, 200)): Promise<SwalModule> {
    if (!isPlatformBrowser(this.platformId)) throw new Error('SwalLoader: browser required');
    if (SwalLoaderService._cached) return SwalLoaderService._cached;

    const root = document.documentElement;
    if (DomAttrGuard.has(root, SwalLoaderService._datasetKey) && SwalLoaderService._cached)
      return SwalLoaderService._cached;

    let last: unknown = null;
    for (let i = 0; i < policy.tries; i++) {
      try {
        const mod = await import('sweetalert2');
        SwalLoaderService._cached = mod;
        DomAttrGuard.set(root, SwalLoaderService._datasetKey, '1');
        return mod;
      } catch (e) {
        last = e;
        await new Promise<void>((r) => setTimeout(r, policy.intervalMs));
      }
    }
    throw last instanceof Error ? last : new Error('SwalLoader: failed');
  }
}
