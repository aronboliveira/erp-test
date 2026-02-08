import { Inject, Injectable } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import type { MeDto } from '../../../lib/interfaces/me.interface';
import { LocalDateMapper } from '../../dates/local-date-mapper.adapter';
import { DateValidator } from '../../dates/date-validator.adapter';

type SwalModule = typeof import('sweetalert2');

@Injectable({ providedIn: 'root' })
export class SwalFacadeService {
  readonly #attrOpen = 'data-profile-modal-open';
  readonly #attrSwalReady = 'data-swal-ready';

  constructor(@Inject(DOCUMENT) private readonly doc: Document) {}

  public async openMeModal(me: MeDto): Promise<void> {
    const body = this.doc?.body;
    if (!body) return;
    if (body.getAttribute(this.#attrOpen) === '1') return;

    body.setAttribute(this.#attrOpen, '1');

    try {
      const Swal = await this.#loadSwal();
      const html = this.#meHtml(me);

      await Swal.default.fire({
        title: 'User profile',
        html,
        confirmButtonText: 'Close',
        focusConfirm: true,
        width: 720,
        didOpen: (popup) => {
          if (!(popup instanceof HTMLElement)) return;
          popup.setAttribute('role', 'dialog');
          popup.setAttribute('aria-modal', 'true');
          popup.setAttribute('aria-label', 'User profile dialog');
        },
      });
    } finally {
      body.setAttribute(this.#attrOpen, '0');
    }
  }

  async #loadSwal(): Promise<SwalModule> {
    const root = this.doc?.documentElement;
    if (root?.getAttribute(this.#attrSwalReady) === '1')
      return this.#retry(() => import('sweetalert2'));

    const mod = await this.#retry(() => import('sweetalert2'));
    root && root.setAttribute(this.#attrSwalReady, '1');
    return mod;
  }

  async #retry<T>(fn: () => Promise<T>, tries = 6, delayMs = 250): Promise<T> {
    let last: unknown = null;

    for (let i = 0; i < tries; i++) {
      try {
        return await fn();
      } catch (e) {
        last = e;
        await this.#delay(delayMs);
      }
    }

    throw last instanceof Error ? last : new Error('Async import failed');
  }

  #delay(ms: number): Promise<void> {
    return new Promise((r) => setTimeout(r, ms));
  }

  #esc(s: string): string {
    return s
      .replaceAll('&', '&amp;')
      .replaceAll('<', '&lt;')
      .replaceAll('>', '&gt;')
      .replaceAll('"', '&quot;')
      .replaceAll("'", '&#39;');
  }

  #meHtml(me: MeDto): string {
    const roles = (me.roleNames ?? []).map((r) => `<li>${this.#esc(r)}</li>`).join('');
    const perms = (me.permissionCodes ?? []).map((p) => `<li>${this.#esc(p)}</li>`).join('');

    const dm = new LocalDateMapper();

    const created = this.#formatDatetimeLocal(me.createdAt, dm);
    const lastLogin = this.#formatDatetimeLocal(me.lastLoginAt, dm);

    return `
      <div class="swal-profile" aria-label="User profile content">
        <div class="swal-profile__grid">
          <div>
            <div class="swal-profile__label">Username</div>
            <div class="swal-profile__value">${this.#esc(me.username ?? '')}</div>
          </div>
          <div>
            <div class="swal-profile__label">Created</div>
            <div class="swal-profile__value">${created}</div>
          </div>
          <div>
            <div class="swal-profile__label">Last login</div>
            <div class="swal-profile__value">${lastLogin}</div>
          </div>
        </div>

        <div class="swal-profile__cols" role="group" aria-label="Roles and permissions">
          <section aria-label="Roles">
            <h3 class="swal-profile__h3">Roles</h3>
            <ul class="swal-profile__list">${roles || '<li>—</li>'}</ul>
          </section>

          <section aria-label="Permissions">
            <h3 class="swal-profile__h3">Permissions</h3>
            <ul class="swal-profile__list">${perms || '<li>—</li>'}</ul>
          </section>
        </div>
      </div>
    `;
  }

  #formatDatetimeLocal(v: string | null | undefined, dm: LocalDateMapper): string {
    if (!v) return '—';
    const parsed = DateValidator.classifyTemporal(v);
    if (!parsed || parsed.kind !== 'datetime-local') return this.#esc(v);

    const d = DateValidator.toDate(parsed.kind, parsed.normalized);
    if (Number.isNaN(d.getTime())) return this.#esc(v);

    const isoWeek = dm.getISOWeekNumber(d);
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    const hh = String(d.getHours()).padStart(2, '0');
    const mi = String(d.getMinutes()).padStart(2, '0');

    return this.#esc(`${yyyy}-${mm}-${dd} ${hh}:${mi} (W${String(isoWeek).padStart(2, '0')})`);
  }
}
