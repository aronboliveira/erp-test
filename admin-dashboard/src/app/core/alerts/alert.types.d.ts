export type AlertKind = 'info' | 'success' | 'warning' | 'error' | 'question';

export type ToastRequest = Readonly<{
  kind: Exclude<AlertKind, 'question'>;
  title: string;
  text?: string;
}>;

export type ConfirmRequest = Readonly<{
  title: string;
  text?: string;
  confirmText?: string;
  cancelText?: string;
}>;
