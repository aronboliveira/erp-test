import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import {
  APP_THEME_PROVIDER,
  DarkThemeProvider,
  LightThemeProvider,
} from './shared/theme/app-theme.providers';
import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { provideHttpClient, withFetch } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch()),
    { provide: APP_THEME_PROVIDER, useClass: LightThemeProvider, multi: true },
    { provide: APP_THEME_PROVIDER, useClass: DarkThemeProvider, multi: true },
  ],
};
