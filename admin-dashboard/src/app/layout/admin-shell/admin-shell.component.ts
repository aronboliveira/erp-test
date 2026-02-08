import { Component, Inject, PLATFORM_ID, signal } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';

import type { SidebarSelection } from '../../lib/interfaces/sidebar.interfaces';
import type { DashboardViewId } from '../../lib/types/dashboard-view.types';

import { SidebarComponent } from '../sidebar/sidebar.component';
import { AppThemeVarsDirective } from '../../shared/directives/app-theme-vars.directive';
import { DarkModeClassDirective } from '../../shared/directives/dark-mode-class.directive';
import { ConfigsPageComponent } from '../../pages/configs-page/configs-page.component';
import { RevenuesPageComponent } from '../../pages/revenues-page/revenues-page.component';
import { ExpensesPageComponent } from '../../pages/expenses-page/expenses-page.component';
import { AppConfigsService } from '../../lib/shared/services/app-configs.service';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [
    CommonModule,
    SidebarComponent,
    AppThemeVarsDirective,
    DarkModeClassDirective,
    ConfigsPageComponent,
    RevenuesPageComponent,
    ExpensesPageComponent,
  ],
  templateUrl: './admin-shell.component.html',
  styleUrl: './admin-shell.component.scss',
})
export class AdminShellComponent {
  readonly #isBrowser: boolean;

  readonly activeViewId = signal<DashboardViewId>('revenues');
  readonly sidebarCollapsed = signal(false);

  constructor(
    @Inject(PLATFORM_ID) platformId: object,
    private readonly cfg: AppConfigsService,
  ) {
    this.#isBrowser = isPlatformBrowser(platformId);

    /**
     * SSR: keep stable default.
     * Browser: restore persisted selection.
     */
    if (this.#isBrowser) this.activeViewId.set(this.cfg.getLastView());
  }

  onSidebarCollapsed(next: boolean): void {
    this.sidebarCollapsed.set(!!next);
  }

  onSelect(sel: SidebarSelection): void {
    const id = sel?.item?.id as DashboardViewId | undefined;
    if (!id) return;

    this.activeViewId.set(id);

    /**
     * Persist selection for next reload.
     */
    this.cfg.setLastView(id);
  }
}
