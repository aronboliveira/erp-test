import type { AlertPort } from './alert.port';
import type { ConfirmRequest, ToastRequest } from './alert.types';

export class SweetAlert2Adapter implements AlertPort {
  async toast(req: ToastRequest): Promise<void> {
    try {
      const Swal = (await import('sweetalert2')).default;
      await Swal.fire({
        icon: req.kind,
        title: req.title,
        text: req.text,
        toast: true,
        position: 'top-end',
        timer: 2200,
        showConfirmButton: false,
      });
    } catch (e) {
      console.error('SweetAlert2Adapter.toast failed', e);
    }
  }

  async confirm(req: ConfirmRequest): Promise<boolean> {
    try {
      const Swal = (await import('sweetalert2')).default;
      const r = await Swal.fire({
        icon: 'question',
        title: req.title,
        text: req.text,
        showCancelButton: true,
        confirmButtonText: req.confirmText || 'Confirm',
        cancelButtonText: req.cancelText || 'Cancel',
      });
      return r.isConfirmed ? true : false;
    } catch (e) {
      console.error('SweetAlert2Adapter.confirm failed', e);
      return false;
    }
  }
}
