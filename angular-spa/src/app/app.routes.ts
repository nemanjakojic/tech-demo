import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { MainComponent } from './main/main.component';
import { authGuard } from './auth.guard';
import { redirectGuard } from './redirect.guard';

export const routes: Routes = 
[
    {
        path: 'login',
        component: LoginComponent
    },
    { 
        path: 'main', 
        component: MainComponent, canActivate: [authGuard] 
    },
    // { 
    //     path: '', 
    //     canActivate: [redirectGuard],
    //     redirectTo: '',
    //     pathMatch: 'full'
    // }
];