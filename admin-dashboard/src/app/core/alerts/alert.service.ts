import { Injectable } from '@angular/core';
import type { AlertPort } from './alert.port';
import type { ConfirmRequest, ToastRequest } from './alert.types';
import { SweetAlert2Adapter } from './sweetalert2.adapter';

@Injectable({ providedIn: 'root' })
export class AlertService {
  readonly #port: AlertPort;

  constructor() {
    this.#port = new SweetAlert2Adapter();
  }

  toast(req: ToastRequest): void {
    void this.#port.toast(req);
  }

  confirm(req: ConfirmRequest): Promise<boolean> {
    return this.#port.confirm(req);
  }
}
