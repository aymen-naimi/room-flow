import {
  addHourCapped,
  bookingRangeFromStart,
  defaultBookingRange,
  minutesForHour,
  roomTone,
  toUtcIso,
  toUtcIsoFromParisLocal,
} from './bookings.helpers';

describe('booking helpers', () => {
  it('converts Date and ISO strings to UTC ISO', () => {
    expect(toUtcIso(new Date('2026-10-25T08:00:00Z'))).toBe('2026-10-25T08:00:00.000Z');
    expect(toUtcIso('2026-10-25T10:00:00+02:00')).toBe('2026-10-25T08:00:00.000Z');
  });

  it('maps a room id to a stable tone bucket', () => {
    expect(roomTone('11111111-1111-1111-1111-111111111111')).toBeGreaterThanOrEqual(0);
    expect(roomTone('11111111-1111-1111-1111-111111111111')).toBeLessThan(4);
    expect(roomTone('a')).toBe(roomTone('a'));
  });

  it('converts a Paris local wall time to UTC ISO across DST', () => {
    expect(toUtcIsoFromParisLocal('2026-08-28', 10, 0)).toBe('2026-08-28T08:00:00.000Z');
    expect(toUtcIsoFromParisLocal('2026-01-15', 10, 0)).toBe('2026-01-15T09:00:00.000Z');
  });

  it('caps a one-hour end at 20:00', () => {
    expect(addHourCapped(10, 0)).toEqual({ hour: 11, minute: 0 });
    expect(addHourCapped(19, 0)).toEqual({ hour: 20, minute: 0 });
    expect(addHourCapped(19, 15)).toEqual({ hour: 20, minute: 0 });
  });

  it('only offers 00 minutes at 20:00', () => {
    expect(minutesForHour(19)).toEqual([0, 15, 30, 45]);
    expect(minutesForHour(20)).toEqual([0]);
  });

  it('builds a one-hour range from a calendar slot start', () => {
    expect(bookingRangeFromStart(new Date('2026-08-28T08:00:00Z'))).toEqual({
      startsAt: '2026-08-28T08:00:00.000Z',
      endsAt: '2026-08-28T09:00:00.000Z',
    });
  });

  it('defaults to the next quarter hour, or 09:00 outside business hours', () => {
    expect(defaultBookingRange(new Date('2026-08-28T06:07:00Z'))).toEqual({
      startsAt: '2026-08-28T06:15:00.000Z',
      endsAt: '2026-08-28T07:15:00.000Z',
    });
    expect(defaultBookingRange(new Date('2026-08-28T05:00:00Z'))).toEqual({
      startsAt: '2026-08-28T07:00:00.000Z',
      endsAt: '2026-08-28T08:00:00.000Z',
    });
    expect(defaultBookingRange(new Date('2026-08-28T18:50:00Z'))).toEqual({
      startsAt: '2026-08-29T07:00:00.000Z',
      endsAt: '2026-08-29T08:00:00.000Z',
    });
  });
});
