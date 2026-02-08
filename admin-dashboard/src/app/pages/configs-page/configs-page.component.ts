import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { AppConfigsService } from '../../lib/shared/services/app-configs.service';
import { DarkModeClassDirective } from '../../shared/directives/dark-mode-class.directive';

@Component({
  selector: 'app-configs-page',
  standalone: true,
  imports: [CommonModule, DarkModeClassDirective],
  templateUrl: './configs-page.component.html',
  styleUrl: './configs-page.component.scss',
})
export class ConfigsPageComponent {
  readonly #isBrowser: boolean;

  constructor(
    @Inject(PLATFORM_ID) platformId: object,
    public readonly cfg: AppConfigsService,
  ) {
    this.#isBrowser = isPlatformBrowser(platformId);
  }

  canPersist(): boolean {
    return this.#isBrowser;
  }

  onToggleDarkMode(v: boolean): void {
    this.cfg.setDarkMode(v);
  }
}
