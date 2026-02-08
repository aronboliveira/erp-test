import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

import type { FormPersistencePort, StoragePort } from './form-persistence.interfaces';
import type { PersistConfig, PersistedFormsState } from './form-persistence.types';

import { BrowserStorageAdapter } from './browser-storage.adapter';

@Injectable({ providedIn: 'root' })
export class FormPersistenceService implements FormPersistencePort {
  static readonly KEY = 'admin.forms.state';

  readonly #cfg: PersistConfig;
  readonly #store: StoragePort;

  #mem: Record<string, Record<string, string>> = {};
  #flushTimer: number | null = null;

  constructor(@Inject(PLATFORM_ID) platformId: object) {
    const isBrowser = isPlatformBrowser(platformId);
    this.#store = new BrowserStorageAdapter(isBrowser);

    this.#cfg = Object.freeze({
      scope: 'local',
      writeMode: 'debounced',
      debounceMs: 250,
      restoreTries: 10,
      restoreDelayMs: 80,
    });

    isBrowser ? this.#hydrate() : void 0;
  }

  snapshot(): PersistedFormsState {
    return Object.freeze(this.#cloneDeep(this.#mem));
  }

  readForm(formId: string): Readonly<Record<string, string>> | null {
    const v = this.#mem[formId];
    return v ? Object.freeze({ ...v }) : null;
  }

  patchField(formId: string, key: string, value: string): void {
    if (!formId || !key) return;

    const curr = this.#mem[formId] || (this.#mem[formId] = {});
    curr[key] === value ? void 0 : (curr[key] = value);

    this.#cfg.writeMode === 'immediate' ? this.#flushNow() : this.#flushDebounced();
  }

  clearForm(formId: string): void {
    if (!formId) return;
    this.#mem[formId] ? delete this.#mem[formId] : void 0;
    this.#flushDebounced();
  }

  get config(): PersistConfig {
    return this.#cfg;
  }

  #hydrate(): void {
    const raw = this.#store.get(this.#cfg.scope, FormPersistenceService.KEY);
    if (!raw) return;

    try {
      const parsed = JSON.parse(raw) as unknown;
      if (!this.#isStateLike(parsed)) return;
      this.#mem = this.#cloneDeep(parsed);
    } catch (e) {
      console.error('FormPersistenceService hydrate failed', e);
    }
  }

  #flushDebounced(): void {
    this.#flushTimer !== null
      ? void 0
      : (this.#flushTimer = window.setTimeout(() => {
          this.#flushTimer = null;
          this.#flushNow();
        }, this.#cfg.debounceMs));
  }

  #flushNow(): void {
    try {
      const json = JSON.stringify(this.#mem);
      this.#store.set(this.#cfg.scope, FormPersistenceService.KEY, json);
    } catch (e) {
      console.error('FormPersistenceService flush failed', e);
    }
  }

  #cloneDeep<T>(o: T): T {
    return JSON.parse(JSON.stringify(o)) as T;
  }

  #isStateLike(v: unknown): v is Record<string, Record<string, string>> {
    if (!v || typeof v !== 'object') return false;
    for (const fk of Object.keys(v as any)) {
      const form = (v as any)[fk];
      if (!form || typeof form !== 'object') return false;
      for (const k of Object.keys(form)) if (typeof form[k] !== 'string') return false;
    }
    return true;
  }
}
