import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { map, take } from 'rxjs/operators';
import { CanActivateFn, Router } from '@angular/router';

export const redirectGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  return authService.isLoggedIn().pipe(
    take(1),
    map(isLoggedIn => {
      return isLoggedIn
        ? router.parseUrl('/main')
        : router.parseUrl('/login');
    })
  );
};
