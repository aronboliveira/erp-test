import { Directive, ElementRef, Inject, PLATFORM_ID, effect } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { UiThemeService } from '../services/ui-theme.service';

@Directive({
  selector: '[appThemeVars]',
  standalone: true,
})
export class AppThemeVarsDirective {
  readonly #isBrowser: boolean;
  readonly #host: HTMLElement;

  constructor(
    @Inject(PLATFORM_ID) platformId: object,
    host: ElementRef<HTMLElement>,
    theme: UiThemeService,
  ) {
    this.#isBrowser = isPlatformBrowser(platformId);
    this.#host = host.nativeElement;

    if (!this.#isBrowser) return;

    effect(() => {
      theme.applyTo(this.#host);
    });
  }
}
