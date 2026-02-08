import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout/app-shell.component').then((m) => m.AppShellComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/dashboard/dashboard-page.component').then(
            (m) => m.DashboardPageComponent,
          ),
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./features/orders/orders-page.component').then((m) => m.OrdersPageComponent),
      },
      {
        path: 'profile',
        loadChildren: () =>
          import('./features/profile/profile.routes').then((m) => m.profileRoutes),
      },
      {
        path: 'billing',
        loadChildren: () =>
          import('./features/billing/billing.routes').then((m) => m.BILLING_ROUTES),
      },

      // existing
      {
        path: 'configs',
        loadComponent: () =>
          import('./pages/configs-page/configs-page.component').then((m) => m.ConfigsPageComponent),
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard-page.component').then(
            (m) => m.DashboardPageComponent,
          ),
      },
      {
        path: 'expenses',
        loadComponent: () =>
          import('./pages/expenses-page/expenses-page.component').then(
            (m) => m.ExpensesPageComponent,
          ),
      },
      {
        path: 'revenues',
        loadComponent: () =>
          import('./pages/revenues-page/revenues-page.component').then(
            (m) => m.RevenuesPageComponent,
          ),
      },

      { path: '**', redirectTo: '' },
    ],
  },
];
