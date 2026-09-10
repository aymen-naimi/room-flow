import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import {
  CalendarOptions,
  DateSelectInfo,
  DatesSetInfo,
  EventClickInfo,
  EventDisplayInfo,
  EventInput,
  FullCalendarModule,
  SlotHeaderInfo,
  SlotLaneInfo,
} from '@fullcalendar/angular';
import interactionPlugin from '@fullcalendar/angular/interaction';
import timeGridPlugin from '@fullcalendar/angular/timegrid';
import themePlugin from '@fullcalendar/angular/themes/monarch';
import frLocale from 'fullcalendar/locales/fr';
import { AuthService } from '../../../core/auth/auth.service';
import { isBookingDayStart, roomTone, toUtcIso } from '../bookings.helpers';

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
    plugins: [themePlugin, timeGridPlugin, interactionPlugin],
    initialView: 'timeGridWeek',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: '',
    },
    locale: frLocale,
    timeZone: 'Europe/Paris',
    height: 'auto',
    slotMinTime: '08:00:00',
    slotMaxTime: '20:00:00',
    slotDuration: '00:15:00',
    allDaySlot: false,
    selectable: true,
    selectMirror: true,
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
    slotLaneClass: (arg: SlotLaneInfo) => this.slotStartClass(arg.date),
    slotHeaderClass: (arg: SlotHeaderInfo) => this.slotStartClass(arg.date),
    selectAllow: (span) => this.isFuture(span.start),
  };

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
