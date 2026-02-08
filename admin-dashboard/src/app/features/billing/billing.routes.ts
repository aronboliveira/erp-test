import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/auth/permission.guard';

export const BILLING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/billing-page.component').then((m) => m.BillingPageComponent),
  },
  {
    path: 'pay',
    loadComponent: () => import('./pages/billing-pay.component').then((m) => m.BillingPayComponent),
    canMatch: [permissionGuard('BILLING_PAY')],
  },
  {
    path: 'success',
    loadComponent: () =>
      import('./pages/billing-success.component').then((m) => m.BillingSuccessComponent),
  },
  {
    path: 'cancel',
    loadComponent: () =>
      import('./pages/billing-cancel.component').then((m) => m.BillingCancelComponent),
  },
];
