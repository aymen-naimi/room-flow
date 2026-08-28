import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { bookingAdaMock, bookingsMock, createBookingRequestMock } from './bookings.mock';
import { BookingsService } from './bookings.service';

describe('BookingsService', () => {
  async function setup(): Promise<{ bookings: BookingsService; http: HttpTestingController }> {
    await TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    return {
      bookings: TestBed.inject(BookingsService),
      http: TestBed.inject(HttpTestingController),
    };
  }

  it('loads bookings for the given range', async () => {
    const { bookings, http } = await setup();
    let result = bookingsMock;

    bookings.getBookings('2026-10-26T00:00:00Z', '2026-11-02T00:00:00Z').subscribe((value) => {
      result = value;
    });

    const request = http.expectOne(
      '/api/bookings?from=2026-10-26T00:00:00Z&to=2026-11-02T00:00:00Z',
    );
    expect(request.request.method).toBe('GET');
    request.flush(bookingsMock);

    expect(result).toEqual(bookingsMock);
    http.verify();
  });

  it('loads bookings filtered by roomId', async () => {
    const { bookings, http } = await setup();
    let result = bookingsMock;

    bookings
      .getBookings('2026-10-26T00:00:00Z', '2026-11-02T00:00:00Z', {
        roomId: bookingAdaMock.roomId,
      })
      .subscribe((value) => {
        result = value;
      });

    const request = http.expectOne(
      `/api/bookings?from=2026-10-26T00:00:00Z&to=2026-11-02T00:00:00Z&roomId=${bookingAdaMock.roomId}`,
    );
    expect(request.request.method).toBe('GET');
    request.flush(bookingsMock);

    expect(result).toEqual(bookingsMock);
    http.verify();
  });

  it('loads the current user bookings with mine=true', async () => {
    const { bookings, http } = await setup();
    let result = bookingsMock;

    bookings
      .getBookings('2026-10-26T00:00:00Z', '2026-11-02T00:00:00Z', { mine: true })
      .subscribe((value) => {
        result = value;
      });

    const request = http.expectOne(
      '/api/bookings?from=2026-10-26T00:00:00Z&to=2026-11-02T00:00:00Z&mine=true',
    );
    expect(request.request.method).toBe('GET');
    request.flush(bookingsMock);

    expect(result).toEqual(bookingsMock);
    http.verify();
  });

  it('creates a booking with the given payload', async () => {
    const { bookings, http } = await setup();
    let result = bookingAdaMock;

    bookings.createBooking(createBookingRequestMock).subscribe((value) => {
      result = value;
    });

    const request = http.expectOne('/api/bookings');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(createBookingRequestMock);
    request.flush(bookingAdaMock);

    expect(result).toEqual(bookingAdaMock);
    http.verify();
  });

  it('deletes a booking by id', async () => {
    const { bookings, http } = await setup();
    let completed = false;

    bookings.deleteBooking(bookingAdaMock.id).subscribe({
      complete: () => {
        completed = true;
      },
    });

    const request = http.expectOne(`/api/bookings/${bookingAdaMock.id}`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null);

    expect(completed).toBe(true);
    http.verify();
  });
});
