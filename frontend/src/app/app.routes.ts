import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'in',
    children: [
      {
        path: 'rooms',
        loadComponent: () =>
          import('./features/rooms/rooms-list/rooms-list').then((m) => m.RoomsList),
      },
    ],
  },
];
