import type { StoragePort } from './form-persistence.interfaces';
import type { PersistScope } from './form-persistence.types';

export class BrowserStorageAdapter implements StoragePort {
  readonly #isBrowser: boolean;

  constructor(isBrowser: boolean) {
    this.#isBrowser = isBrowser;
  }

  get(scope: PersistScope, key: string): string | null {
    if (!this.#isBrowser) return null;
    try {
      const store = scope === 'session' ? window.sessionStorage : window.localStorage;
      return store.getItem(key);
    } catch (e) {
      console.error('BrowserStorageAdapter.get failed', e);
      return null;
    }
  }

  set(scope: PersistScope, key: string, value: string): void {
    if (!this.#isBrowser) return;
    try {
      const store = scope === 'session' ? window.sessionStorage : window.localStorage;
      store.setItem(key, value);
    } catch (e) {
      console.error('BrowserStorageAdapter.set failed', e);
    }
  }

  remove(scope: PersistScope, key: string): void {
    if (!this.#isBrowser) return;
    try {
      const store = scope === 'session' ? window.sessionStorage : window.localStorage;
      store.removeItem(key);
    } catch (e) {
      console.error('BrowserStorageAdapter.remove failed', e);
    }
  }
}
