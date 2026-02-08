import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { SwalLoaderService } from '../../../core/ui/swal-loader.service';
import { DarkModeClassDirective } from '../../../shared/directives/dark-mode-class.directive';

@Component({
  standalone: true,
  selector: 'app-billing-cancel',
  imports: [DarkModeClassDirective],
  template: `
    <section appDarkModeClass class="page page--billing" aria-label="Billing cancel">
      <header class="page__header">
        <div>
          <div class="page__titleRow">
            <span class="page__titleIcon" aria-hidden="true">×</span>
            <h1 class="page__title">Payment canceled</h1>
          </div>
          <p class="page__subtitle">Checkout was canceled before completion.</p>
        </div>
      </header>

      <section class="panel">
        <div class="panel__body">
          <div class="billing-result billing-result--cancel" role="status" aria-live="polite">
            <div class="billing-result__icon" aria-hidden="true">×</div>
            <div class="billing-result__title">Canceled</div>
            <div class="billing-result__text">No charges were made to your account.</div>
          </div>
        </div>
      </section>
    </section>
  `,
})
export class BillingCancelComponent {
  constructor(
    private readonly swal: SwalLoaderService,
    @Inject(PLATFORM_ID) private readonly platformId: object,
  ) {}

  async ngOnInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    const Swal = (await this.swal.load()).default;
    await Swal.fire({ icon: 'info', title: 'Canceled', text: 'Checkout was canceled.' });
  }
}
