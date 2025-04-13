import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from './auth.service';
import { map, take } from 'rxjs/operators';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);

  return authService.isLoggedIn().pipe(
    take(1),
    map(isLoggedIn => isLoggedIn)
  );
};
