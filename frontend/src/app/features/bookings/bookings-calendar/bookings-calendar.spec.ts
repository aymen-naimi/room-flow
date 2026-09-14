import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '../../../core/auth/auth.service';
import { loginResponseMock } from '../../../core/auth/auth.mock';
import { BookingsCalendar } from './bookings-calendar';

describe('BookingsCalendar', () => {
  async function setup(): Promise<{ nativeElement: HTMLElement; calendar: BookingsCalendar }> {
    await TestBed.configureTestingModule({
      imports: [BookingsCalendar],
      providers: [
        {
          provide: AuthService,
          useValue: { currentUser: signal(loginResponseMock.user) },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(BookingsCalendar);
    fixture.componentRef.setInput('events', []);
    fixture.detectChanges();

    return { nativeElement: fixture.nativeElement, calendar: fixture.componentInstance };
  }

  it('renders full-calendar with an empty event list', async () => {
    const { nativeElement } = await setup();

    expect(nativeElement.querySelector('full-calendar')).toBeTruthy();
    expect(nativeElement.querySelector('[aria-label="Planning des réservations"]')).toBeTruthy();
  });

  it('sets an aria-label on mounted events', async () => {
    const { calendar } = await setup();
    const el = document.createElement('div');

    calendar['calendarOptions'].eventDidMount?.({
      el,
      event: {
        title: 'Jane Doe',
        start: new Date('2026-08-28T08:00:00.000Z'),
        end: new Date('2026-08-28T09:00:00.000Z'),
        extendedProps: { mine: true, ariaName: 'Jane Doe' },
      },
    } as never);

    expect(el.getAttribute('aria-label')).toBe('Vous, Jane Doe, 10:00–11:00');
  });
});
