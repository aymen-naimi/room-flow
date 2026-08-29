import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, UrlTree } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './auth.guard';
import { AUTH_SESSION_KEY } from './auth.service';
import { adminLoginResponseMock, loginResponseMock } from './auth.mock';

describe('auth guards', () => {
  afterEach(() => {
    sessionStorage.clear();
  });

  async function setup(session: typeof loginResponseMock | null = loginResponseMock): Promise<void> {
    sessionStorage.clear();
    if (session) {
      sessionStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(session));
    }

    await TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
  }

  it('redirects anonymous users to login', async () => {
    await setup(null);
    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/login');
  });

  it('allows authenticated users into the app', async () => {
    await setup();
    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(result).toBe(true);
  });

  it('sends authenticated users away from login', async () => {
    await setup();
    const result = TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/in/my-bookings');
  });

  it('redirects anonymous users away from admin routes', async () => {
    await setup(null);
    const result = TestBed.runInInjectionContext(() => adminGuard({} as never, {} as never));
    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/login');
  });

  it('redirects authenticated non-admin users to rooms', async () => {
    await setup();
    const result = TestBed.runInInjectionContext(() => adminGuard({} as never, {} as never));
    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/in/rooms');
  });

  it('allows admins into admin routes', async () => {
    await setup(adminLoginResponseMock);
    const result = TestBed.runInInjectionContext(() => adminGuard({} as never, {} as never));
    expect(result).toBe(true);
  });
});
