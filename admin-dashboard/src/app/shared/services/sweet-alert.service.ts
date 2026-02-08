import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

type SwalModule = typeof import('sweetalert2');

@Injectable({ providedIn: 'root' })
export class SweetAlertService {
  static readonly _attrImporting = 'data-swal-importing';
  static readonly _evtReady = 'app:swal-ready';

  readonly #isBrowser: boolean;
  #mod: SwalModule | null = null;

  constructor(@Inject(PLATFORM_ID) platformId: object) {
    this.#isBrowser = isPlatformBrowser(platformId);
  }

  async showProfileStub(payload: {
    displayName?: string | null;
    email?: string | null;
  }): Promise<void> {
    if (!this.#isBrowser) return;

    const mod = await this.#load();
    if (!mod) return;

    const title = payload?.displayName ? payload.displayName : 'User Profile';
    const email = payload?.email ? payload.email : '—';

    await mod.default.fire({
      title,
      html: `<div style="text-align:left">
        <div><strong>Email:</strong> ${this.#escape(email)}</div>
        <div style="margin-top:8px; opacity:.8">Routes/API binding will be wired later.</div>
      </div>`,
      icon: 'info',
      confirmButtonText: 'OK',
      showCloseButton: true,
    });
  }

  async #load(): Promise<SwalModule | null> {
    if (this.#mod) return this.#mod;

    const root = document.documentElement;

    if (root.hasAttribute(SweetAlertService._attrImporting)) return this.#waitReady(900);

    root.setAttribute(SweetAlertService._attrImporting, '1');

    try {
      const mod = await this.#retry(async () => import('sweetalert2'), 5, 180);
      this.#mod = mod;

      root.removeAttribute(SweetAlertService._attrImporting);
      window.dispatchEvent(new Event(SweetAlertService._evtReady));

      return mod;
    } catch (e) {
      root.removeAttribute(SweetAlertService._attrImporting);
      console.error('SweetAlert2 dynamic import failed', e);
      return null;
    }
  }

  #waitReady(timeoutMs: number): Promise<SwalModule | null> {
    if (this.#mod) return Promise.resolve(this.#mod);

    return new Promise((resolve) => {
      let done = false;

      const onReady = () => {
        if (done) return;
        done = true;
        window.removeEventListener(SweetAlertService._evtReady, onReady as any);
        resolve(this.#mod);
      };

      window.addEventListener(SweetAlertService._evtReady, onReady as any);

      setTimeout(() => {
        if (done) return;
        done = true;
        window.removeEventListener(SweetAlertService._evtReady, onReady as any);
        resolve(this.#mod);
      }, timeoutMs);
    });
  }

  #retry<T>(fn: () => Promise<T>, tries: number, delayMs: number): Promise<T> {
    return new Promise((resolve, reject) => {
      let n = 0;

      const attempt = () => {
        n += 1;
        fn()
          .then(resolve)
          .catch((e) => {
            n >= tries ? reject(e) : setTimeout(attempt, delayMs);
          });
      };

      attempt();
    });
  }

  #escape(v: string): string {
    return v
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }
}
