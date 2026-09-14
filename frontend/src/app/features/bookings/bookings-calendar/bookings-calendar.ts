import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import {
  CalendarOptions,
  DateSelectInfo,
  DatesSetInfo,
  EventClickInfo,
  EventDisplayInfo,
  EventInput,
  FullCalendarModule,
  MountInfo,
  SlotHeaderInfo,
  SlotLaneInfo,
} from '@fullcalendar/angular';
import { AuthService } from '../../../core/auth/auth.service';
import { bookingEventAriaLabel, isBookingDayStart, roomTone, toUtcIso } from '../bookings.helpers';
import { BookingsCalendarBaseOptions } from './bookings-calendar.options';

@Component({
  selector: 'app-bookings-calendar',
  imports: [FullCalendarModule],
  templateUrl: './bookings-calendar.html',
  styleUrl: './bookings-calendar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingsCalendar {
  private readonly auth = inject(AuthService);

  readonly events = input.required<EventInput[]>();
  readonly datesSet = output<DatesSetInfo>();
  readonly selectRange = output<DateSelectInfo>();
  readonly eventClick = output<EventClickInfo>();

  protected readonly calendarOptions: CalendarOptions = {
    ...BookingsCalendarBaseOptions,
    datesSet: (info) => {
      this.datesSet.emit(info);
    },
    select: (info) => {
      this.selectRange.emit(info);
    },
    eventClick: (info) => {
      this.eventClick.emit(info);
    },
    eventClass: (arg: EventDisplayInfo) => this.eventClassNames(arg.event.extendedProps).join(' '),
    eventDidMount: (info: MountInfo<EventDisplayInfo>) => this.setEventAriaLabel(info),
    slotLaneClass: (arg: SlotLaneInfo) => this.slotStartClass(arg.date),
    slotHeaderClass: (arg: SlotHeaderInfo) => this.slotStartClass(arg.date),
    selectAllow: (span) => this.isFuture(span.start),
  };

  private setEventAriaLabel(info: MountInfo<EventDisplayInfo>): void {
    const start = info.event.start;
    const end = info.event.end;
    if (!start || !end) {
      return;
    }

    const mine = Boolean(info.event.extendedProps['mine']);
    const name = String(info.event.extendedProps['ariaName'] ?? info.event.title);
    info.el.setAttribute('aria-label', bookingEventAriaLabel(mine, name, start, end));
  }

  private eventClassNames(extendedProps: Record<string, unknown>): string[] {
    const currentUserId = this.auth.currentUser()?.id;
    const userId = String(extendedProps['userId'] ?? '');
    const roomId = String(extendedProps['roomId'] ?? '');
    const mine = currentUserId !== undefined && userId === currentUserId;
    return [
      mine ? 'bookings__event--mine' : 'bookings__event--other',
      `bookings__event--room-${roomTone(roomId)}`,
    ];
  }

  private slotStartClass(date: Date): string {
    return isBookingDayStart(date) ? 'bookings__slot--start' : '';
  }

  private isFuture(start: unknown): boolean {
    return new Date(toUtcIso(start)).getTime() >= Date.now();
  }
}
