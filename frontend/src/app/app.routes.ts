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
    loadComponent: () => import('./core/main-layout/main-layout').then((m) => m.MainLayout),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'my-bookings',
      },
      {
        path: 'bookings',
        loadComponent: () =>
          import('./features/bookings/bookings/bookings').then((m) => m.Bookings),
        data: { mode: 'room' },
      },
      {
        path: 'bookings/:roomId',
        loadComponent: () =>
          import('./features/bookings/bookings/bookings').then((m) => m.Bookings),
        data: { mode: 'room' },
      },
      {
        path: 'my-bookings',
        loadComponent: () =>
          import('./features/bookings/bookings/bookings').then((m) => m.Bookings),
        data: { mode: 'mine' },
      },
      {
        path: 'rooms',
        loadComponent: () =>
          import('./features/rooms/rooms-list/rooms-list').then((m) => m.RoomsList),
      },
      {
        path: 'rooms/new',
        loadComponent: () =>
          import('./features/rooms/rooms-create/rooms-create').then((m) => m.RoomsCreate),
      },
    ],
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'in/my-bookings',
  },
  {
    path: '**',
    redirectTo: 'in/my-bookings',
  },
];
