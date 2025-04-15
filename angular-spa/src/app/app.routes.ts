import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { MainComponent } from './main/main.component';
import { authGuard } from './auth.guard';
import { redirectGuard } from './redirect.guard';
import { RedirectComponent } from './redirect/redirect.component';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    canActivate: [redirectGuard],
    component: RedirectComponent, // required but won't render
  },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'main',
    component: MainComponent,
    canActivate: [authGuard],
  },
  // {
  //     path: '',
  //     canActivate: [redirectGuard],
  //     redirectTo: '',
  //     pathMatch: 'full'
  // }
];
