import type { ConfirmRequest, ToastRequest } from './alert.types';

export interface AlertPort {
  toast(req: ToastRequest): Promise<void>;
  confirm(req: ConfirmRequest): Promise<boolean>;
}
