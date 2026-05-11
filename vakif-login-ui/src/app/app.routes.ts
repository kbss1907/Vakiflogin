import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { HomeComponent } from './pages/home/home.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'leave/my-requests', loadComponent: () => import('./pages/leave/my-requests/my-requests.component').then(m => m.MyRequestsComponent), canActivate: [authGuard] },
  { path: 'leave/pending', loadComponent: () => import('./pages/leave/pending-requests/pending-requests.component').then(m => m.PendingRequestsComponent), canActivate: [authGuard] },
  { path: 'employee/list', loadComponent: () => import('./pages/employee/list/list.component').then(m => m.ListComponent), canActivate: [authGuard] },
  { path: 'employee/:id', loadComponent: () => import('./pages/employee/detail/detail.component').then(m => m.DetailComponent), canActivate: [authGuard] },
  { path: 'forgot-password', loadComponent: () => import('./pages/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent) },
  { path: 'reset-password', loadComponent: () => import('./pages/reset-password/reset-password.component').then(m => m.ResetPasswordComponent) },
  { path: '', component: HomeComponent, canActivate: [authGuard] }, // Korumalı Ana Sayfa
  { path: '**', redirectTo: '' } // Bilinmeyen sayfaları ana sayfaya (dolayısıyla logine) atar
];