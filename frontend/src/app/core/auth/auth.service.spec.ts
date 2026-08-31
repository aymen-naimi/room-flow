import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { adminLoginResponseMock, loginResponseMock } from './auth.mock';
import { AUTH_SESSION_KEY, AuthService } from './auth.service';

describe('AuthService', () => {
  afterEach(() => {
    sessionStorage.clear();
  });

  async function setup(): Promise<{ auth: AuthService; http: HttpTestingController }> {
    sessionStorage.clear();
    await TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    return {
      auth: TestBed.inject(AuthService),
      http: TestBed.inject(HttpTestingController),
    };
  }

  it('stores the user after login without putting tokens in sessionStorage', async () => {
    const { auth, http } = await setup();

    auth.login({ email: 'jane.doe@example.com', password: 'password1' }).subscribe();
    http.expectOne('/api/auth/login').flush(loginResponseMock);

    expect(auth.isAuthenticated()).toBe(true);
    expect(auth.isAdmin()).toBe(false);
    expect(auth.accessToken()).toBe(loginResponseMock.accessToken);
    expect(auth.currentUser()?.email).toBe(loginResponseMock.user.email);
    expect(sessionStorage.getItem(AUTH_SESSION_KEY)).toContain(loginResponseMock.user.email);
    expect(sessionStorage.getItem(AUTH_SESSION_KEY)).not.toContain(loginResponseMock.accessToken);
    http.verify();
  });

  it('registers then logs in', async () => {
    const { auth, http } = await setup();

    auth
      .registerAndLogin({
        email: 'jane.doe@example.com',
        password: 'password1',
        firstName: 'Jane',
        lastName: 'Doe',
      })
      .subscribe();

    http.expectOne('/api/auth/register').flush(loginResponseMock.user);
    http.expectOne('/api/auth/login').flush(loginResponseMock);

    expect(auth.isAuthenticated()).toBe(true);
    http.verify();
  });

  it('rotates the access token on refresh', async () => {
    const { auth, http } = await setup();

    auth.login({ email: 'jane.doe@example.com', password: 'password1' }).subscribe();
    http.expectOne('/api/auth/login').flush(loginResponseMock);

    const rotated = {
      ...loginResponseMock,
      accessToken: 'access-2',
    };
    auth.refresh().subscribe();
    const refresh = http.expectOne('/api/auth/refresh');
    expect(refresh.request.body).toEqual({});
    refresh.flush(rotated);

    expect(auth.accessToken()).toBe('access-2');
    http.verify();
  });

  it('clears the session on logout', async () => {
    const { auth, http } = await setup();

    auth.login({ email: 'jane.doe@example.com', password: 'password1' }).subscribe();
    http.expectOne('/api/auth/login').flush(loginResponseMock);

    auth.logout().subscribe();
    http.expectOne('/api/auth/logout').flush(null, { status: 204, statusText: 'No Content' });

    expect(auth.isAuthenticated()).toBe(false);
    expect(auth.isAdmin()).toBe(false);
    expect(sessionStorage.getItem(AUTH_SESSION_KEY)).toBeNull();
    http.verify();
  });

  it('treats an admin session as admin', async () => {
    const { auth, http } = await setup();

    auth.login({ email: 'jane.doe@example.com', password: 'password1' }).subscribe();
    http.expectOne('/api/auth/login').flush(adminLoginResponseMock);

    expect(auth.isAdmin()).toBe(true);
    http.verify();
  });
});
