import { Component } from '@angular/core';
import { DarkModeClassDirective } from '../../shared/directives/dark-mode-class.directive';
import { DashboardChartsComponent } from './charts/dashboard-charts.component';

@Component({
  standalone: true,
  selector: 'app-dashboard-page',
  imports: [DarkModeClassDirective, DashboardChartsComponent],
  template: `
    <section appDarkModeClass class="page page--dashboard" aria-label="Dashboard overview">
      <header class="page__header">
        <div>
          <div class="page__titleRow">
            <span class="page__titleIcon" aria-hidden="true">
              <svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
                <path
                  d="M12 3.75a.75.75 0 0 1 .531.22l7.5 7.5a.75.75 0 0 1-1.062 1.06l-.862-.86v7.58a.75.75 0 0 1-.75.75h-10.5a.75.75 0 0 1-.75-.75v-7.58l-.862.86a.75.75 0 0 1-1.062-1.06l7.5-7.5a.75.75 0 0 1 .53-.22z"
                  fill="currentColor"
                ></path>
              </svg>
            </span>
            <h1 class="page__title">Dashboard</h1>
          </div>
          <p class="page__subtitle">KPIs, revenue, conversion, operational alerts.</p>
        </div>
      </header>
      <app-dashboard-charts />
    </section>
  `,
})
export class DashboardPageComponent {}
