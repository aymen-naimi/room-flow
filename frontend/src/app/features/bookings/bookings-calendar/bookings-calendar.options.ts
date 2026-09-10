import { CalendarOptions } from '@fullcalendar/angular';
import interactionPlugin from '@fullcalendar/angular/interaction';
import timeGridPlugin from '@fullcalendar/angular/timegrid';
import themePlugin from '@fullcalendar/angular/themes/monarch';
import frLocale from 'fullcalendar/locales/fr';
import { BOOKING_HOUR_END, BOOKING_HOUR_START, BOOKING_TIME_ZONE } from '../bookings.helpers';

function hourAsSlotTime(hour: number): string {
  return `${String(hour).padStart(2, '0')}:00:00`;
}

export const BookingsCalendarBaseOptions: CalendarOptions = {
  plugins: [themePlugin, timeGridPlugin, interactionPlugin],
  initialView: 'timeGridWeek',
  headerToolbar: {
    left: 'prev,next today',
    center: 'title',
    right: '',
  },
  locale: frLocale,
  timeZone: BOOKING_TIME_ZONE,
  height: 'auto',
  slotMinTime: hourAsSlotTime(BOOKING_HOUR_START),
  slotMaxTime: hourAsSlotTime(BOOKING_HOUR_END),
  slotDuration: '00:15:00',
  allDaySlot: false,
  selectable: true,
  selectMirror: true,
};
