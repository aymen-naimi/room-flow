import { Room } from '../rooms/rooms.model';

export interface Booking {
  id: string;
  roomId: string;
  roomName: string;
  userId: string;
  userDisplayName: string;
  startsAt: string;
  endsAt: string;
}

export interface CreateBookingRequest {
  roomId: string;
  startsAt: string;
  endsAt: string;
}

export interface GetBookingsFilter {
  roomId?: string;
  mine?: boolean;
}

export interface BookingRange {
  startsAt: string;
  endsAt: string;
}

export interface BookingDraft {
  date: Date;
  startHour: number;
  startMinute: number;
  endHour: number;
  endMinute: number;
}

export interface BookingsCreateDialogData {
  rooms: Room[];
  startsAt: string;
  endsAt: string;
  roomId?: string;
}
