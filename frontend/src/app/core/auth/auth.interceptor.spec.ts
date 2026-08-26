import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { authInterceptor } from './auth.interceptor';
import { loginResponseMock } from './auth.mock';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  afterEach(() => {
    sessionStorage.clear();
  });

  async function setup(): Promise<{
    httpClient: HttpClient;
    http: HttpTestingController;
    auth: AuthService;
  }> {
    sessionStorage.clear();
    await TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    return {
      httpClient: TestBed.inject(HttpClient),
      http: TestBed.inject(HttpTestingController),
      auth: TestBed.inject(AuthService),
    };
  }

  async function login(auth: AuthService, http: HttpTestingController): Promise<void> {
    auth.login({ email: 'jane.doe@example.com', password: 'password1' }).subscribe();
    http.expectOne('/api/auth/login').flush(loginResponseMock);
  }

  it('adds the bearer token to protected requests', async () => {
    const { httpClient, http, auth } = await setup();
    await login(auth, http);

    httpClient.get('/api/rooms').subscribe();
    const req = http.expectOne('/api/rooms');
    expect(req.request.headers.get('Authorization')).toBe(
      `Bearer ${loginResponseMock.accessToken}`,
    );
    req.flush([]);
    http.verify();
  });

  it('does not add the bearer token to auth endpoints', async () => {
    const { httpClient, http, auth } = await setup();
    await login(auth, http);

    httpClient.post('/api/auth/refresh', { refreshToken: 'refresh-token' }).subscribe();
    const req = http.expectOne('/api/auth/refresh');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush(loginResponseMock);
    http.verify();
  });

  it('refreshes once then retries the original request', async () => {
    const { httpClient, http, auth } = await setup();
    await login(auth, http);

    let rooms: unknown;
    httpClient.get('/api/rooms').subscribe((value) => {
      rooms = value;
    });

    http.expectOne('/api/rooms').flush(null, { status: 401, statusText: 'Unauthorized' });
    http.expectOne('/api/auth/refresh').flush({
      ...loginResponseMock,
      accessToken: 'access-2',
      refreshToken: 'refresh-2',
    });

    const retry = http.expectOne('/api/rooms');
    expect(retry.request.headers.get('Authorization')).toBe('Bearer access-2');
    retry.flush([{ id: '1' }]);

    expect(rooms).toEqual([{ id: '1' }]);
    http.verify();
  });

  it('shares a single refresh across concurrent 401s', async () => {
    const { httpClient, http, auth } = await setup();
    await login(auth, http);

    httpClient.get('/api/rooms/a').subscribe();
    httpClient.get('/api/rooms/b').subscribe();

    http.expectOne('/api/rooms/a').flush(null, { status: 401, statusText: 'Unauthorized' });
    http.expectOne('/api/rooms/b').flush(null, { status: 401, statusText: 'Unauthorized' });

    const refreshCalls = http.match('/api/auth/refresh');
    expect(refreshCalls.length).toBe(1);
    refreshCalls[0].flush({
      ...loginResponseMock,
      accessToken: 'access-2',
      refreshToken: 'refresh-2',
    });

    http.expectOne('/api/rooms/a').flush({ id: 'a' });
    http.expectOne('/api/rooms/b').flush({ id: 'b' });
    http.verify();
  });

  it('does not retry a failed refresh', async () => {
    const { httpClient, http, auth } = await setup();
    await login(auth, http);

    let failed = false;
    httpClient.post('/api/auth/refresh', { refreshToken: 'refresh-token' }).subscribe({
      error: () => {
        failed = true;
      },
    });

    http.expectOne('/api/auth/refresh').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(failed).toBe(true);
    http.verify();
  });
});
