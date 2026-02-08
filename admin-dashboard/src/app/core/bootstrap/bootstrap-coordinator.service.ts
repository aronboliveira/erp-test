import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { DatasetGuard } from '../dom/dataset-guard';

@Injectable({ providedIn: 'root' })
export class BootstrapCoordinatorService {
  readonly #isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) platformId: object) {
    this.#isBrowser = isPlatformBrowser(platformId);
  }

  wireOnce(): void {
    if (!this.#isBrowser) return;

    const root = document.documentElement;
    DatasetGuard.once(root, 'wired', () => this.#retry(() => this.#wireWindowOnce(), 8, 90));
  }

  #wireWindowOnce(): boolean {
    if (!document.body?.isConnected) return false;

    DatasetGuard.once(window.document.body, 'window-events', () => {
      window.addEventListener('online', () => void 0, { passive: true });
      window.addEventListener('offline', () => void 0, { passive: true });
    });

    return true;
  }

  #retry(op: () => boolean, tries: number, delayMs: number): void {
    let left = Math.max(1, Math.trunc(tries));
    const tick = () => {
      const ok = (() => {
        try {
          return op();
        } catch (e) {
          console.error('bootstrap op failed', e);
          return false;
        }
      })();
      ok ? void 0 : --left > 0 ? setTimeout(tick, delayMs) : void 0;
    };
    tick();
  }
}
