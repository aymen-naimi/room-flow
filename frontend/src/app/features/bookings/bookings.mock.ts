import { Booking, CreateBookingRequest } from './bookings.model';

export const bookingAdaMock: Booking = {
  id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  roomId: '11111111-1111-1111-1111-111111111111',
  roomName: 'Salle Horizon',
  userId: '11111111-1111-1111-1111-111111111111',
  userDisplayName: 'Jane Doe',
  startsAt: '2026-10-26T08:00:00Z',
  endsAt: '2026-10-26T09:00:00Z',
};

export const bookingOtherMock: Booking = {
  id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
  roomId: '22222222-2222-2222-2222-222222222222',
  roomName: 'Salle Nord',
  userId: '33333333-3333-3333-3333-333333333333',
  userDisplayName: 'Bob Martin',
  startsAt: '2026-10-26T10:00:00Z',
  endsAt: '2026-10-26T11:00:00Z',
};

export const createBookingRequestMock: CreateBookingRequest = {
  roomId: bookingAdaMock.roomId,
  startsAt: bookingAdaMock.startsAt,
  endsAt: bookingAdaMock.endsAt,
};

export const bookingsMock: Booking[] = [bookingAdaMock, bookingOtherMock];
