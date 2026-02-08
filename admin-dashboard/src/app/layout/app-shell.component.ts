import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';
import { TopbarComponent } from './topbar.component';
import { AppThemeVarsDirective } from '../shared/directives/app-theme-vars.directive';
import { DarkModeClassDirective } from '../shared/directives/dark-mode-class.directive';
import { BootstrapCoordinatorService } from '../core/bootstrap/bootstrap-coordinator.service';

@Component({
  standalone: true,
  selector: 'app-shell',
  imports: [RouterOutlet, SidebarComponent, TopbarComponent, AppThemeVarsDirective, DarkModeClassDirective],
  template: `
    <div
      class="app-shell"
      appThemeVars
      appDarkModeClass
      [style.--sidebar-col]="sidebarCollapsed() ? '5rem' : '18.5rem'"
    >
      <a class="app-shell__skip" href="#main">Skip to content</a>

      <div class="app-shell__grid">
        <app-sidebar
          class="app-shell__sidebar"
          (collapsedChange)="onSidebarCollapsed($event)"
        ></app-sidebar>

        <div class="app-shell__content">
          <app-topbar class="app-shell__topbar"></app-topbar>

          <main id="main" class="app-shell__main" role="main" aria-label="Admin content" tabindex="-1">
            <router-outlet></router-outlet>
          </main>
        </div>
      </div>
    </div>
  `,
})
export class AppShellComponent {
  readonly #bootstrap = inject(BootstrapCoordinatorService);
  readonly sidebarCollapsed = signal(false);

  constructor() {
    this.#bootstrap.wireOnce();
  }

  onSidebarCollapsed(next: boolean): void {
    this.sidebarCollapsed.set(!!next);
  }
}
