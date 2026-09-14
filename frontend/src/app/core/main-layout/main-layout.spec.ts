import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter, RouterLinkActive } from '@angular/router';
import { vi } from 'vitest';
import { loginResponseMock } from '../auth/auth.mock';
import { AuthService } from '../auth/auth.service';
import { MainLayout } from './main-layout';

describe('MainLayout', () => {
  async function setup(): Promise<{ fixture: ComponentFixture<MainLayout> }> {
    await TestBed.configureTestingModule({
      imports: [MainLayout],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            currentUser: signal(loginResponseMock.user),
            logout: vi.fn(),
          },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(MainLayout);
    fixture.detectChanges();
    return { fixture };
  }

  it('exposes a skip link to the main content', async () => {
    const { fixture } = await setup();
    const skip = fixture.nativeElement.querySelector('.main-layout__skip');

    expect(skip?.getAttribute('href')).toBe('#contenu');
    expect(skip?.textContent).toContain('Aller au contenu');
    expect(fixture.nativeElement.querySelector('main#contenu')).toBeTruthy();
  });

  it('marks the active nav link as the current page', async () => {
    const { fixture } = await setup();
    const actives = fixture.debugElement.queryAll(By.directive(RouterLinkActive));
    const byHref = (href: string) =>
      actives.find((link) => link.nativeElement.getAttribute('href') === href)?.injector.get(
        RouterLinkActive,
      );

    expect(actives).toHaveLength(3);
    expect(
      actives.every((link) => link.injector.get(RouterLinkActive).ariaCurrentWhenActive === 'page'),
    ).toBe(true);
    expect(isExactMatch(byHref('/in/my-bookings'))).toBe(false);
    expect(isExactMatch(byHref('/in/bookings'))).toBe(false);
    expect(isExactMatch(byHref('/in/rooms'))).toBe(true);
  });
});

function isExactMatch(link: RouterLinkActive | undefined): boolean {
  const options = link?.routerLinkActiveOptions;
  return !!options && 'exact' in options && options.exact === true;
}
