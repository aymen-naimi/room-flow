import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { AuthService } from '../../../core/auth/auth.service';
import { loginResponseMock } from '../../../core/auth/auth.mock';
import { ConfirmDialog } from '../../../core/confirm-dialog/confirm-dialog';
import { Toast } from '../../../core/toast';
import { Room } from '../../rooms/rooms.model';
import { roomHorizonMock, roomsMock } from '../../rooms/rooms.mock';
import {
  BookingDeleteSuccessMessage,
  Bookings,
  BookingsAgendaTitle,
  BookingsDisponibilitesTitle,
  BookingsMode,
  BookingsRoomMode,
} from './bookings';
import { BookingsCreateDialog } from '../bookings-create-dialog/bookings-create-dialog';
import { bookingAdaMock, bookingOtherMock, bookingsMock } from '../bookings.mock';

function isBookingsListRequest(request: { method: string; url: string }): boolean {
  return request.method === 'GET' && request.url.split('?')[0] === '/api/bookings';
}

function activatedRoute(mode: BookingsMode, roomId?: string) {
  const paramMap = convertToParamMap(roomId ? { roomId } : {});
  return {
    snapshot: {
      data: { mode },
      paramMap,
    },
    paramMap: of(paramMap),
  };
}

describe('Bookings', () => {
  async function flushBookingsList(
    http: HttpTestingController,
    body = bookingsMock,
  ): Promise<ReturnType<HttpTestingController['expectOne']> | undefined> {
    for (let attempt = 0; attempt < 30; attempt++) {
      const request = http.match(isBookingsListRequest)[0];
      if (request) {
        request.flush(body);
        return request;
      }

      await new Promise((resolve) => setTimeout(resolve, 10));
    }

    return undefined;
  }

  async function setup(
    options: {
      confirmed?: boolean;
      mode?: BookingsMode;
      roomId?: string;
      rooms?: Room[];
    } = {},
  ): Promise<{
    fixture: ReturnType<typeof TestBed.createComponent<Bookings>>;
    http: HttpTestingController;
    toast: { error: ReturnType<typeof vi.fn>; success: ReturnType<typeof vi.fn> };
    openDialog: ReturnType<typeof vi.fn>;
    bookingsRequest: ReturnType<HttpTestingController['expectOne']> | undefined;
  }> {
    const confirmed = options.confirmed ?? true;
    const mode = options.mode ?? BookingsRoomMode;
    const rooms = options.rooms ?? roomsMock;
    const toast = { error: vi.fn(), success: vi.fn() };
    const openDialog = vi.fn(() => ({ afterClosed: () => of(confirmed) }));

    await TestBed.configureTestingModule({
      imports: [Bookings],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([
          { path: 'in/bookings', component: Bookings },
          { path: 'in/bookings/:roomId', component: Bookings },
          { path: 'in/my-bookings', component: Bookings },
          { path: 'in/rooms/new', component: Bookings },
        ]),
        {
          provide: ActivatedRoute,
          useValue: activatedRoute(mode, options.roomId),
        },
        {
          provide: MatDialog,
          useValue: { open: openDialog },
        },
        { provide: Toast, useValue: toast },
        {
          provide: AuthService,
          useValue: { currentUser: signal(loginResponseMock.user) },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(Bookings);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('/api/rooms').flush(rooms);
    await fixture.whenStable();
    fixture.detectChanges();
    const bookingsRequest = await flushBookingsList(http);
    await fixture.whenStable();
    fixture.detectChanges();

    return { fixture, http, toast, openDialog, bookingsRequest };
  }

  it('shows a link to create a room when none exist', async () => {
    const toast = { error: vi.fn(), success: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [Bookings],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: 'in/rooms/new', component: Bookings }]),
        {
          provide: ActivatedRoute,
          useValue: activatedRoute(BookingsRoomMode),
        },
        { provide: MatDialog, useValue: { open: vi.fn() } },
        { provide: Toast, useValue: toast },
        {
          provide: AuthService,
          useValue: { currentUser: signal(loginResponseMock.user) },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(Bookings);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('/api/rooms').flush([]);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Aucune salle pour le moment.');
    expect(fixture.nativeElement.querySelector('.bookings__link').getAttribute('href')).toBe(
      '/in/rooms/new',
    );
    expect(fixture.nativeElement.querySelector('.bookings__reserve')).toBeNull();
    http.verify();
  });

  it('loads room bookings after rooms and maps events by user name', async () => {
    const { fixture, http, bookingsRequest } = await setup({ roomId: roomHorizonMock.id });

    expect(fixture.nativeElement.querySelector('.page__title').textContent).toContain(
      BookingsDisponibilitesTitle,
    );
    expect(fixture.nativeElement.querySelector('full-calendar')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.bookings__empty')).toBeNull();
    expect(fixture.nativeElement.querySelector('.bookings__room')).toBeTruthy();
    expect(fixture.nativeElement.textContent).toContain(
      `${roomHorizonMock.name} (${roomHorizonMock.capacity} places)`,
    );
    expect(fixture.nativeElement.querySelector('.bookings__reserve').disabled).toBe(false);
    const events = fixture.componentInstance['events']();
    expect(events.some((event) => event.id === bookingAdaMock.id)).toBe(true);
    expect(events.find((event) => event.id === bookingAdaMock.id)?.title).toBe(
      bookingAdaMock.userDisplayName,
    );
    expect(bookingsRequest?.request.params.get('roomId')).toBe(roomHorizonMock.id);
    http.verify();
  });

  it('keeps the room selector empty when no roomId is in the route', async () => {
    const { fixture, http, bookingsRequest } = await setup();

    expect(fixture.nativeElement.querySelector('.bookings__room')).toBeTruthy();
    expect(fixture.componentInstance['selectedRoomId']()).toBeNull();
    expect(fixture.nativeElement.querySelector('.bookings__empty')).toBeTruthy();
    expect(fixture.nativeElement.textContent).toContain('Choisissez une salle');
    expect(fixture.nativeElement.querySelector('full-calendar')).toBeNull();
    expect(fixture.nativeElement.querySelector('.bookings__reserve').disabled).toBe(true);
    expect(bookingsRequest).toBeUndefined();
    http.verify();
  });

  it('loads the current user bookings without a room selector', async () => {
    const { fixture, http, bookingsRequest } = await setup({ mode: 'mine' });

    expect(fixture.nativeElement.querySelector('.page__title').textContent).toContain(
      BookingsAgendaTitle,
    );
    expect(fixture.nativeElement.querySelector('.bookings__room')).toBeNull();
    expect(bookingsRequest?.request.params.get('mine')).toBe('true');
    expect(bookingsRequest?.request.params.get('roomId')).toBeNull();
    expect(
      fixture.componentInstance['events']().find((event) => event.id === bookingAdaMock.id)?.title,
    ).toBe(`${bookingAdaMock.roomName} (${roomHorizonMock.capacity} places)`);
    http.verify();
  });

  it('deletes an owned booking after confirmation', async () => {
    const { fixture, http, openDialog, toast } = await setup({ roomId: roomHorizonMock.id });
    const pending = fixture.componentInstance['onEventClick']({
      event: {
        id: bookingAdaMock.id,
        title: bookingAdaMock.userDisplayName,
        extendedProps: { userId: bookingAdaMock.userId, roomId: bookingAdaMock.roomId },
      },
    } as never);
    await fixture.whenStable();

    expect(openDialog).toHaveBeenCalledWith(
      ConfirmDialog,
      expect.objectContaining({
        data: expect.objectContaining({ destructive: true }),
      }),
    );

    const request = http.expectOne(`/api/bookings/${bookingAdaMock.id}`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null, { status: 204, statusText: 'No Content' });
    const reload = await flushBookingsList(http, [bookingOtherMock]);
    await pending;

    expect(reload).toBeDefined();
    expect(toast.success).toHaveBeenCalledWith(BookingDeleteSuccessMessage);
  });

  it('does not delete another user booking', async () => {
    const { fixture, http, openDialog } = await setup({ roomId: roomHorizonMock.id });

    await fixture.componentInstance['onEventClick']({
      event: {
        id: bookingOtherMock.id,
        title: bookingOtherMock.userDisplayName,
        extendedProps: { userId: bookingOtherMock.userId, roomId: bookingOtherMock.roomId },
      },
    } as never);

    expect(openDialog).not.toHaveBeenCalled();
    http.verify();
  });

  it('opens the create dialog from a calendar slot with a 15-minute end', async () => {
    const { fixture, openDialog } = await setup({ mode: 'mine', confirmed: false });
    const unselect = vi.fn();

    await fixture.componentInstance['onSelect']({
      start: new Date('2026-08-28T08:00:00Z'),
      end: new Date('2026-08-28T08:15:00Z'),
      view: { calendar: { unselect } },
    } as never);

    expect(unselect).toHaveBeenCalled();
    expect(openDialog).toHaveBeenCalledWith(
      BookingsCreateDialog,
      expect.objectContaining({
        data: expect.objectContaining({
          startsAt: '2026-08-28T08:00:00.000Z',
          endsAt: '2026-08-28T08:15:00.000Z',
        }),
      }),
    );
  });

  it('opens the create dialog with the dragged calendar range', async () => {
    const { fixture, openDialog } = await setup({ mode: 'mine', confirmed: false });
    const unselect = vi.fn();

    await fixture.componentInstance['onSelect']({
      start: new Date('2026-08-28T08:00:00Z'),
      end: new Date('2026-08-28T10:00:00Z'),
      view: { calendar: { unselect } },
    } as never);

    expect(unselect).toHaveBeenCalled();
    expect(openDialog).toHaveBeenCalledWith(
      BookingsCreateDialog,
      expect.objectContaining({
        data: expect.objectContaining({
          startsAt: '2026-08-28T08:00:00.000Z',
          endsAt: '2026-08-28T10:00:00.000Z',
        }),
      }),
    );
  });

  it('opens the create dialog from the reserve button', async () => {
    const { fixture, openDialog } = await setup({ mode: 'mine', confirmed: false });

    fixture.nativeElement.querySelector('.bookings__reserve').click();
    await fixture.whenStable();

    expect(openDialog).toHaveBeenCalledWith(
      BookingsCreateDialog,
      expect.objectContaining({
        data: expect.objectContaining({
          rooms: roomsMock,
        }),
      }),
    );
  });
});
