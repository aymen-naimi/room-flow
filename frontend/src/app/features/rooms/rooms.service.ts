import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Room } from './room';
import { Observable } from 'rxjs';

@Service()
export class RoomsService {
  private readonly http = inject(HttpClient);

  public getRooms(): Observable<Room[]> {
    return this.http.get<Room[]>('/api/rooms');
  }
}
