import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { SwalLoaderService } from '../../../core/ui/swal-loader.service';
import { DarkModeClassDirective } from '../../../shared/directives/dark-mode-class.directive';

@Component({
  standalone: true,
  selector: 'app-billing-success',
  imports: [DarkModeClassDirective],
  template: `
    <section appDarkModeClass class="page page--billing" aria-label="Billing success">
      <header class="page__header">
        <div>
          <div class="page__titleRow">
            <span class="page__titleIcon" aria-hidden="true">✓</span>
            <h1 class="page__title">Payment confirmed</h1>
          </div>
          <p class="page__subtitle">Stripe returned success.</p>
        </div>
      </header>

      <section class="panel">
        <div class="panel__body">
          <div class="billing-result billing-result--success" role="status" aria-live="polite">
            <div class="billing-result__icon" aria-hidden="true">✓</div>
            <div class="billing-result__title">Success</div>
            <div class="billing-result__text">Your checkout completed successfully.</div>
          </div>
        </div>
      </section>
    </section>
  `,
})
export class BillingSuccessComponent {
  constructor(
    private readonly swal: SwalLoaderService,
    @Inject(PLATFORM_ID) private readonly platformId: object,
  ) {}

  async ngOnInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    const Swal = (await this.swal.load()).default;
    await Swal.fire({
      icon: 'success',
      title: 'Payment confirmed',
      text: 'Stripe returned success.',
    });
  }
}
