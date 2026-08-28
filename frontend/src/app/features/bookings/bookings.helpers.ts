import { BookingDraft, BookingRange } from './bookings.model';

export const BOOKING_TIME_ZONE = 'Europe/Paris';
export const BOOKING_MINUTES = [0, 15, 30, 45] as const;
export const BOOKING_HOUR_START = 8;
export const BOOKING_HOUR_END = 20;
export const BOOKING_HOURS = Array.from(
  { length: BOOKING_HOUR_END - BOOKING_HOUR_START + 1 },
  (_, index) => BOOKING_HOUR_START + index,
);

export function toUtcIso(value: unknown): string {
  if (value instanceof Date) {
    return value.toISOString();
  }

  if (typeof value === 'string') {
    return parseIso(value);
  }

  if (isEpochMillis(value)) {
    return new Date(value.epochMilliseconds).toISOString();
  }

  if (value && typeof value === 'object' && typeof value.toString === 'function') {
    return parseIso(value.toString());
  }

  throw new Error('Unsupported calendar date');
}

export function minutesForHour(hour: number): number[] {
  return hour === BOOKING_HOUR_END ? [0] : [...BOOKING_MINUTES];
}

export function toYmdFromDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function toDateFromYmd(dateYmd: string): Date {
  const [year, month, day] = dateYmd.split('-').map(Number);
  return new Date(year, month - 1, day);
}

export function toUtcIsoFromParisLocal(dateYmd: string, hour: number, minute: number): string {
  return toUtcIso(fromParisLocal(dateYmd, hour, minute));
}

export function bookingRangeFromStart(start: unknown): BookingRange {
  const wall = parisWallTime(new Date(toUtcIso(start)));
  const snapped = snapQuarter(wall.hour, wall.minute);
  const end = addHourCapped(snapped.hour, snapped.minute);
  return {
    startsAt: toUtcIsoFromParisLocal(wall.date, snapped.hour, snapped.minute),
    endsAt: toUtcIsoFromParisLocal(wall.date, end.hour, end.minute),
  };
}

export function defaultBookingRange(now = new Date()): BookingRange {
  const start = nextQuarterInBusinessHours(now);
  const end = addHourCapped(start.hour, start.minute);
  return {
    startsAt: toUtcIsoFromParisLocal(start.date, start.hour, start.minute),
    endsAt: toUtcIsoFromParisLocal(start.date, end.hour, end.minute),
  };
}

export function bookingDraftFromIso(startsAt: string, endsAt: string): BookingDraft {
  const start = parisWallTime(new Date(startsAt));
  const end = parisWallTime(new Date(endsAt));
  return {
    date: toDateFromYmd(start.date),
    startHour: start.hour,
    startMinute: start.minute,
    endHour: end.hour,
    endMinute: end.minute,
  };
}

export function parisTodayDate(now = new Date()): Date {
  return toDateFromYmd(parisWallTime(now).date);
}

export function addHourCapped(hour: number, minute: number): { hour: number; minute: number } {
  const endHour = hour + 1;
  if (endHour > BOOKING_HOUR_END || (endHour === BOOKING_HOUR_END && minute > 0)) {
    return { hour: BOOKING_HOUR_END, minute: 0 };
  }

  return { hour: endHour, minute };
}

function nextQuarterInBusinessHours(now: Date): { date: string; hour: number; minute: number } {
  const wall = parisWallTime(now);
  let totalMinutes = wall.hour * 60 + wall.minute;
  if (wall.second > 0 || now.getMilliseconds() > 0) {
    totalMinutes += 1;
  }

  const ceiled = Math.ceil(totalMinutes / 15) * 15;
  let hour = Math.floor(ceiled / 60);
  let minute = ceiled % 60;
  let dateYmd = wall.date;
  const lastStartMinutes = (BOOKING_HOUR_END - 1) * 60 + 45;

  if (hour < BOOKING_HOUR_START) {
    hour = 9;
    minute = 0;
  } else if (hour * 60 + minute > lastStartMinutes) {
    dateYmd = addDaysYmd(dateYmd, 1);
    hour = 9;
    minute = 0;
  }

  return { date: dateYmd, hour, minute };
}

function snapQuarter(hour: number, minute: number): { hour: number; minute: number } {
  let snappedMinute = Math.round(minute / 15) * 15;
  let snappedHour = hour;
  if (snappedMinute === 60) {
    snappedHour += 1;
    snappedMinute = 0;
  }

  return { hour: snappedHour, minute: snappedMinute };
}

function addDaysYmd(dateYmd: string, days: number): string {
  const date = toDateFromYmd(dateYmd);
  date.setDate(date.getDate() + days);
  return toYmdFromDate(date);
}

function parisWallTime(value: Date): {
  date: string;
  hour: number;
  minute: number;
  second: number;
} {
  const parts = new Intl.DateTimeFormat('en-GB', {
    timeZone: BOOKING_TIME_ZONE,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hourCycle: 'h23',
  }).formatToParts(value);
  const map = Object.fromEntries(
    parts.filter((part) => part.type !== 'literal').map((part) => [part.type, part.value]),
  );

  return {
    date: `${map['year']}-${map['month']}-${map['day']}`,
    hour: Number(map['hour']),
    minute: Number(map['minute']),
    second: Number(map['second']),
  };
}

function fromParisLocal(dateYmd: string, hour: number, minute: number): Date {
  const [year, month, day] = dateYmd.split('-').map(Number);
  const intended = Date.UTC(year, month - 1, day, hour, minute, 0);
  let instant = intended;
  for (let pass = 0; pass < 2; pass++) {
    const wall = parisWallTime(new Date(instant));
    const actual = Date.UTC(
      Number(wall.date.slice(0, 4)),
      Number(wall.date.slice(5, 7)) - 1,
      Number(wall.date.slice(8, 10)),
      wall.hour,
      wall.minute,
      wall.second,
    );
    instant += intended - actual;
  }

  return new Date(instant);
}

export function roomTone(roomId: string): number {
  let hash = 0;
  for (const char of roomId) {
    hash = (hash + char.charCodeAt(0)) % 4;
  }
  return hash;
}

function parseIso(value: string): string {
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    throw new Error('Unsupported calendar date');
  }
  return parsed.toISOString();
}

function isEpochMillis(value: unknown): value is { epochMilliseconds: number } {
  return (
    !!value &&
    typeof value === 'object' &&
    'epochMilliseconds' in value &&
    typeof (value as { epochMilliseconds: unknown }).epochMilliseconds === 'number'
  );
}
