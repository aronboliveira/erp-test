import { Component, inject } from '@angular/core';
import { AlertService } from '../core/alerts/alert.service';

@Component({
  standalone: true,
  selector: 'app-topbar',
  styleUrl: './topbar.component.scss',
  template: `
    <header class="topbar" role="banner" aria-label="Top bar">
      <div class="topbar__inner">
        <div class="topbar__meta">
          <div class="topbar__eyebrow">Administrative Dashboard</div>
          <div class="topbar__subtitle">Operations and monitoring</div>
        </div>

        <div class="topbar__actions">
          <button
            type="button"
            class="topbar__button"
            aria-label="Show readiness notification"
            (click)="notifyReady()"
          >
            <span class="topbar__icon" aria-hidden="true">
              <svg viewBox="0 0 16 16" focusable="false" aria-hidden="true">
                <path
                  d="M8 16a2 2 0 0 0 1.985-1.75h-3.97A2 2 0 0 0 8 16z"
                ></path>
                <path
                  d="M8 1a4 4 0 0 0-4 4v1.086c0 .49-.195.96-.541 1.306l-.85.85A1 1 0 0 0 3.318 10h9.364a1 1 0 0 0 .707-1.707l-.85-.85A1.85 1.85 0 0 1 12 6.086V5a4 4 0 0 0-4-4z"
                ></path>
              </svg>
            </span>
            Notify
          </button>
        </div>
      </div>
    </header>
  `,
})
export class TopbarComponent {
  readonly #alerts = inject(AlertService);

  notifyReady(): void {
    this.#alerts.toast({ kind: 'success', title: 'Ready', text: 'Shell initialized.' });
  }
}
