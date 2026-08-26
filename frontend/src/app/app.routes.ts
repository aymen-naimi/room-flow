import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    path: 'in',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./core/main-layout/main-layout').then((m) => m.MainLayout),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'rooms',
      },
      {
        path: 'rooms',
        loadComponent: () =>
          import('./features/rooms/rooms-list/rooms-list').then((m) => m.RoomsList),
      },
    ],
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'in/rooms',
  },
  {
    path: '**',
    redirectTo: 'in/rooms',
  },
];
