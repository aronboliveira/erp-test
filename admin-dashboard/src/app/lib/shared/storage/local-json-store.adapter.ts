import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ObjectMapper } from '../../../shared/utils/object-mapper.adapter';

type JsonRecord = Record<string, any>;

@Injectable({ providedIn: 'root' })
export class LocalJsonStore<T extends JsonRecord> {
  readonly #isBrowser: boolean;
  readonly #key: string;

  constructor(@Inject(PLATFORM_ID) platformId: object) {
    this.#isBrowser = isPlatformBrowser(platformId);
    this.#key = '';
  }

  configure(key: string): LocalJsonStore<T> {
    (this as any).#key = key;
    return this;
  }

  read(fallback: T): T {
    if (!this.#isBrowser) return ObjectMapper.deepSeal({ ...fallback });
    if (!this.#key) return ObjectMapper.deepSeal({ ...fallback });

    try {
      const raw = localStorage.getItem(this.#key);
      if (!raw) return ObjectMapper.deepSeal({ ...fallback });

      const parsed = JSON.parse(raw);
      if (!parsed || typeof parsed !== 'object') return ObjectMapper.deepSeal({ ...fallback });

      const merged = { ...fallback, ...parsed } as T;
      return ObjectMapper.deepSeal(merged);
    } catch (e) {
      console.warn('LocalJsonStore.read failed', e);
      return ObjectMapper.deepSeal({ ...fallback });
    }
  }

  write(value: T): void {
    if (!this.#isBrowser) return;
    if (!this.#key) return;

    try {
      const json = JSON.stringify(value);
      localStorage.setItem(this.#key, json);
    } catch (e) {
      console.warn('LocalJsonStore.write failed', e);
    }
  }

  has(): boolean {
    if (!this.#isBrowser) return false;
    if (!this.#key) return false;

    try {
      return localStorage.getItem(this.#key) !== null ? true : false;
    } catch (_) {
      return false;
    }
  }
}
