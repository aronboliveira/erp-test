import { inject } from '@angular/core';
import { CanMatchFn, Router, UrlTree } from '@angular/router';
import { map, catchError, of } from 'rxjs';
import { AccessStoreService } from '../../features/profile/services/access-store.service';

export const permissionGuard =
  (required: string): CanMatchFn =>
  () => {
    const router = inject(Router);
    const access = inject(AccessStoreService);

    return access.me$().pipe(
      map((me) => (access.hasPermission(me, required) ? true : router.parseUrl('/dashboard'))),
      catchError(() => of(router.parseUrl('/dashboard') as UrlTree)),
    );
  };
