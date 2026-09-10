import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '../../../core/auth/auth.service';
import { loginResponseMock } from '../../../core/auth/auth.mock';
import { BookingsCalendar } from './bookings-calendar';

describe('BookingsCalendar', () => {
  it('renders full-calendar with an empty event list', async () => {
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

    expect(fixture.nativeElement.querySelector('full-calendar')).toBeTruthy();
  });
});
