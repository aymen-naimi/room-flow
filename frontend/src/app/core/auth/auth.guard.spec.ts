import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, UrlTree } from '@angular/router';
import { authGuard, guestGuard } from './auth.guard';
import { AUTH_SESSION_KEY } from './auth.service';
import { loginResponseMock } from './auth.mock';

describe('auth guards', () => {
  afterEach(() => {
    sessionStorage.clear();
  });

  async function setup(authenticated: boolean): Promise<void> {
    sessionStorage.clear();
    if (authenticated) {
      sessionStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(loginResponseMock));
    }

    await TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
  }

  it('redirects anonymous users to login', async () => {
    await setup(false);
    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/login');
  });

  it('allows authenticated users into the app', async () => {
    await setup(true);
    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(result).toBe(true);
  });

  it('sends authenticated users away from login', async () => {
    await setup(true);
    const result = TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/in/rooms');
  });
});
