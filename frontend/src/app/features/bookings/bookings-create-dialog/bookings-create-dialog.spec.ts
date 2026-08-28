import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';
import { roomsMock } from '../../rooms/rooms.mock';
import { BookingCreateErrorMessage, BookingsCreateDialog } from './bookings-create-dialog';
import { bookingAdaMock, createBookingRequestMock } from '../bookings.mock';

const composedCreateRequest = {
  roomId: createBookingRequestMock.roomId,
  startsAt: '2026-10-26T08:00:00.000Z',
  endsAt: '2026-10-26T09:00:00.000Z',
};

describe('BookingsCreateDialog', () => {
  async function setup(roomId?: string): Promise<{
    fixture: ComponentFixture<BookingsCreateDialog>;
    http: HttpTestingController;
    close: ReturnType<typeof vi.fn>;
  }> {
    const close = vi.fn();

    await TestBed.configureTestingModule({
      imports: [BookingsCreateDialog],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatDialogRef, useValue: { close } },
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            rooms: roomsMock,
            startsAt: createBookingRequestMock.startsAt,
            endsAt: createBookingRequestMock.endsAt,
            roomId,
          },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(BookingsCreateDialog);
    fixture.detectChanges();

    return { fixture, http: TestBed.inject(HttpTestingController), close };
  }

  it('posts the selected room and times and shows a conflict message on 409', async () => {
    const { fixture, http, close } = await setup();
    fixture.componentInstance['form'].controls.roomId.setValue(createBookingRequestMock.roomId);
    fixture.detectChanges();

    fixture.nativeElement.querySelector('.bookings-create__submit').click();
    await fixture.whenStable();

    const request = http.expectOne('/api/bookings');
    expect(request.request.body).toEqual(composedCreateRequest);
    request.flush({ title: 'Conflict' }, { status: 409, statusText: 'Conflict' });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(BookingCreateErrorMessage.Overlap);
    expect(close).not.toHaveBeenCalled();
    http.verify();
  });

  it('posts startsAt and endsAt from the time selectors', async () => {
    const { fixture, http, close } = await setup();
    const form = fixture.componentInstance['form'];
    form.controls.roomId.setValue(createBookingRequestMock.roomId);
    form.controls.endHour.setValue(12);
    form.controls.endMinute.setValue(0);
    fixture.detectChanges();

    fixture.nativeElement.querySelector('.bookings-create__submit').click();
    await fixture.whenStable();

    const request = http.expectOne('/api/bookings');
    expect(request.request.body).toEqual({
      roomId: createBookingRequestMock.roomId,
      startsAt: '2026-10-26T08:00:00.000Z',
      endsAt: '2026-10-26T11:00:00.000Z',
    });
    request.flush(bookingAdaMock);
    await fixture.whenStable();

    expect(close).toHaveBeenCalledWith(bookingAdaMock);
    http.verify();
  });

  it('shows an error when the end is not after the start', async () => {
    const { fixture, http } = await setup();
    const form = fixture.componentInstance['form'];
    form.controls.roomId.setValue(createBookingRequestMock.roomId);
    form.patchValue({ startHour: 10, startMinute: 0, endHour: 10, endMinute: 0 });
    form.updateValueAndValidity();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(BookingCreateErrorMessage.Range);
    expect(fixture.nativeElement.querySelector('.bookings-create__submit').disabled).toBe(true);

    await fixture.componentInstance['submit']();
    http.expectNone('/api/bookings');
    http.verify();
  });

  it('closes with the created booking on success', async () => {
    const { fixture, http, close } = await setup();
    fixture.componentInstance['form'].controls.roomId.setValue(createBookingRequestMock.roomId);
    fixture.detectChanges();
    fixture.nativeElement.querySelector('.bookings-create__submit').click();
    await fixture.whenStable();

    http.expectOne('/api/bookings').flush(bookingAdaMock);
    await fixture.whenStable();

    expect(close).toHaveBeenCalledWith(bookingAdaMock);
    http.verify();
  });

  it('hides the room select when a room is locked and posts that room', async () => {
    const { fixture, http, close } = await setup(createBookingRequestMock.roomId);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.bookings-create__room')).toBeTruthy();
    expect(fixture.nativeElement.textContent).toContain(roomsMock[0].name);

    fixture.nativeElement.querySelector('.bookings-create__submit').click();
    await fixture.whenStable();

    const request = http.expectOne('/api/bookings');
    expect(request.request.body).toEqual(composedCreateRequest);
    request.flush(bookingAdaMock);
    await fixture.whenStable();

    expect(close).toHaveBeenCalledWith(bookingAdaMock);
    http.verify();
  });
});
