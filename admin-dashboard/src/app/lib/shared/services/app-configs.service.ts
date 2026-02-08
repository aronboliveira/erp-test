import { Inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { AppConfigs } from '../../interfaces/app-configs.interfaces';
import { Flag01 } from '../../types/flags.types';
import { DashboardViewId } from '../../types/dashboard-view.types';
import { LocalJsonStore } from '../storage/local-json-store.adapter';

@Injectable({ providedIn: 'root' })
export class AppConfigsService {
  static readonly STORAGE_KEY = '_app_fin_ptXsT_cfgs';

  readonly #isBrowser: boolean;
  readonly #store: LocalJsonStore<AppConfigs>;

  readonly configs: ReturnType<typeof signal<AppConfigs>>;

  constructor(@Inject(PLATFORM_ID) platformId: object, store: LocalJsonStore<AppConfigs>) {
    this.#isBrowser = isPlatformBrowser(platformId);
    this.#store = store.configure(AppConfigsService.STORAGE_KEY);
    this.configs = signal<AppConfigs>(this.#init());

    this.#isBrowser ? this.#persistIfMissing() : null;
  }

  isDarkMode(): boolean {
    return this.configs().dark_mode === '1';
  }

  setDarkMode(v: boolean): void {
    const next: AppConfigs = { ...this.configs(), dark_mode: (v ? '1' : '0') as Flag01 };
    this.#commit(next);
  }

  toggleDarkMode(): void {
    this.setDarkMode(!this.isDarkMode());
  }

  getLastView(): DashboardViewId {
    return this.#normalizeView(this.configs().last_view);
  }

  setLastView(v: DashboardViewId): void {
    const next: AppConfigs = { ...this.configs(), last_view: this.#normalizeView(v) };
    this.#commit(next);
  }

  #commit(next: AppConfigs): void {
    const normalized: AppConfigs = {
      ...next,
      dark_mode: next.dark_mode === '1' ? '1' : '0',
      last_view: this.#normalizeView(next.last_view),
    };

    this.configs.set(normalized);
    this.#isBrowser ? this.#store.write(normalized) : null;
  }

  #persistIfMissing(): void {
    this.#store.has() ? null : this.#store.write(this.configs());
  }

  #init(): AppConfigs {
    const fallback: AppConfigs = {
      dark_mode: this.#prefersDark() ? '1' : '0',
      last_view: 'revenues',
    };

    const fromStore = this.#store.read(fallback);

    return {
      ...fromStore,
      dark_mode: fromStore.dark_mode === '1' ? '1' : '0',
      last_view: this.#normalizeView(fromStore.last_view),
    };
  }

  #normalizeView(v: any): DashboardViewId {
    return v === 'configs' || v === 'expenses' || v === 'revenues' ? v : 'revenues';
  }

  #prefersDark(): boolean {
    if (!this.#isBrowser) return false;

    try {
      const mq = window.matchMedia?.('(prefers-color-scheme: dark)');
      return mq?.matches ? true : false;
    } catch (_) {
      return false;
    }
  }
}
