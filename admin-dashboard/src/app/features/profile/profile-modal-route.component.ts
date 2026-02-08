import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { ApiClientService } from '../../core/api/api-client.service';
import { SwalFacadeService } from '../../core/ui/swal/swal-facade.service';
import ExceptionHandler from '../../shared/utils/exception-handler.util';

@Component({
  standalone: true,
  template: '',
})
export class ProfileModalRouteComponent implements OnInit, OnDestroy {
  readonly #api = inject(ApiClientService);
  readonly #router = inject(Router);
  readonly #swal = inject(SwalFacadeService);

  #sub: Subscription | null = null;

  ngOnInit(): void {
    this.#sub = this.#api.getMe().subscribe({
      next: async (me) => {
        try {
          await this.#swal.openMeModal(me);
        } catch (e) {
          e instanceof Error ? ExceptionHandler.logUnexpected(e) : null;
        } finally {
          this.#router.navigateByUrl('/');
        }
      },
      error: async (e) => {
        try {
          await this.#router.navigateByUrl('/');
        } finally {
          e instanceof Error ? ExceptionHandler.logUnexpected(e) : null;
        }
      },
    });
  }

  ngOnDestroy(): void {
    this.#sub?.unsubscribe();
    this.#sub = null;
  }
}
