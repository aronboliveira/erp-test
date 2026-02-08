import { Inject, Injectable, PLATFORM_ID, computed, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

import type { AppThemeTokens, ThemeProvider } from '../../lib/interfaces/theme.interfaces';
import type { ThemeMode } from '../../lib/types/theme.types';

import { AppConfigsService } from '../../lib/shared/services/app-configs.service';
import { ThemeVarFlattener } from '../adapters/theme-var-flattener.adapter';
import { APP_THEME_PROVIDER } from '../theme/app-theme.providers';
import { DARK_APP_THEME, LIGHT_APP_THEME } from '../theme/app-theme.tokens';

@Injectable({ providedIn: 'root' })
export class UiThemeService {
  static readonly ATTR = 'data-app-theme';
  static readonly ATTR_APPLIED = 'data-app-theme-applied';

  readonly #isBrowser: boolean;
  readonly #providers: readonly ThemeProvider<AppThemeTokens>[];

  readonly mode = computed<ThemeMode>(() => (this.cfg.isDarkMode() ? 'dark' : 'light'));

  readonly tokens = computed<AppThemeTokens>(() => {
    const m = this.mode();
    const p = this.#providers.find((x) => x?.supports(m));
    return p ? p.provide(m) : m === 'dark' ? DARK_APP_THEME : LIGHT_APP_THEME;
  });

  readonly cssVars = computed(() => ThemeVarFlattener.toCssVars(this.tokens()));

  constructor(
    @Inject(PLATFORM_ID) platformId: object,
    public readonly cfg: AppConfigsService,
  ) {
    this.#isBrowser = isPlatformBrowser(platformId);

    const injected = inject(APP_THEME_PROVIDER, { optional: true }) as any;
    this.#providers = Array.isArray(injected) ? injected : [];
  }

  applyTo(el: HTMLElement): void {
    if (!this.#isBrowser) return;

    const mode = this.mode();
    const tag = `${mode}-v1`;

    if (el.hasAttribute(UiThemeService.ATTR) && el.getAttribute(UiThemeService.ATTR) === tag)
      return;

    try {
      el.setAttribute(UiThemeService.ATTR, tag);
      this.#applyVars(el, this.cssVars());
      el.hasAttribute(UiThemeService.ATTR_APPLIED)
        ? null
        : el.setAttribute(UiThemeService.ATTR_APPLIED, '1');
    } catch (e) {
      console.error('UiThemeService.applyTo failed', e);
    }
  }

  #applyVars(el: HTMLElement, vars: Readonly<Record<string, string>>): void {
    for (const k of Object.keys(vars)) {
      const key = k as any;
      const next = vars[key];
      const prev = el.style.getPropertyValue(key).trim();
      prev === next ? null : el.style.setProperty(key, next);
    }
  }
}
