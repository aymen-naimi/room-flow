import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { Booking, CreateBookingRequest, GetBookingsFilter } from './bookings.model';

@Service()
export class BookingsService {
  private readonly http = inject(HttpClient);

  public getBookings(from: string, to: string, filter?: GetBookingsFilter): Observable<Booking[]> {
    let params = new HttpParams().set('from', from).set('to', to);
    if (filter?.roomId) {
      params = params.set('roomId', filter.roomId);
    }
    if (filter?.mine) {
      params = params.set('mine', 'true');
    }

    return this.http.get<Booking[]>('/api/bookings', { params });
  }

  public createBooking(request: CreateBookingRequest): Observable<Booking> {
    return this.http.post<Booking>('/api/bookings', request);
  }

  public deleteBooking(id: string): Observable<void> {
    return this.http.delete<void>(`/api/bookings/${id}`);
  }
}
