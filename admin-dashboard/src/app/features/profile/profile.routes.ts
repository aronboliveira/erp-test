import type { Routes } from '@angular/router';

export const profileRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./profile-modal-route.component').then((m) => m.ProfileModalRouteComponent),
  },
];
