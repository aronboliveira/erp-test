import { InjectionToken } from '@angular/core';
import type { ThemeMode } from '../../lib/types/theme.types';
import type { AppThemeTokens, ThemeProvider } from '../../lib/interfaces/theme.interfaces';
import { DARK_APP_THEME, LIGHT_APP_THEME } from './app-theme.tokens';

export const APP_THEME_PROVIDER = new InjectionToken<ThemeProvider<AppThemeTokens>>(
  'APP_THEME_PROVIDER',
);

export class LightThemeProvider implements ThemeProvider<AppThemeTokens> {
  supports(mode: ThemeMode): boolean {
    return mode === 'light';
  }

  provide(): AppThemeTokens {
    return LIGHT_APP_THEME;
  }
}

export class DarkThemeProvider implements ThemeProvider<AppThemeTokens> {
  supports(mode: ThemeMode): boolean {
    return mode === 'dark';
  }

  provide(): AppThemeTokens {
    return DARK_APP_THEME;
  }
}
