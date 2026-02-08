import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { isPlatformBrowser } from '@angular/common';
import { debounceTime, Subscription } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class FormPersistService {
  readonly #isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) platformId: object) {
    this.#isBrowser = isPlatformBrowser(platformId);
  }

  restore<T extends Record<string, any>>(key: string, fg: FormGroup<T>): void {
    if (!this.#isBrowser) return;
    try {
      const raw = localStorage.getItem(key);
      if (!raw) return;
      const data = JSON.parse(raw);
      fg.patchValue(data);
    } catch {}
  }

  bind<T extends Record<string, any>>(key: string, fg: FormGroup<T>): Subscription | null {
    if (!this.#isBrowser) return null;
    return fg.valueChanges.pipe(debounceTime(200)).subscribe((v) => {
      try {
        localStorage.setItem(key, JSON.stringify(v));
      } catch {}
    });
  }

  clear(key: string): void {
    if (!this.#isBrowser) return;
    try {
      localStorage.removeItem(key);
    } catch {}
  }
}
