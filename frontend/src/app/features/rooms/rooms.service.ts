import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { CreateRoomRequest, Room } from './room';
import { Observable } from 'rxjs';

@Service()
export class RoomsService {
  private readonly http = inject(HttpClient);

  public getRooms(): Observable<Room[]> {
    return this.http.get<Room[]>('/api/rooms');
  }

  public createRoom(request: CreateRoomRequest): Observable<Room> {
    return this.http.post<Room>('/api/rooms', request);
  }

  public deleteRoom(id: string): Observable<void> {
    return this.http.delete<void>(`/api/rooms/${id}`);
  }
}
