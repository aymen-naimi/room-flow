import { HttpClient } from '@angular/common/http';
import { computed, inject, Service, signal } from '@angular/core';
import {
  catchError,
  finalize,
  Observable,
  of,
  shareReplay,
  switchMap,
  tap,
  throwError,
} from 'rxjs';
import { LoginRequest, LoginResponse, RegisterRequest, User } from './auth.models';

export const AUTH_SESSION_KEY = 'room-flow.session';

interface StoredSession {
  accessToken: string;
  refreshToken: string;
  user: User;
}

@Service()
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly userSignal = signal<User | null>(null);
  private readonly accessTokenSignal = signal<string | null>(null);
  private readonly refreshTokenSignal = signal<string | null>(null);
  private refreshInFlight$: Observable<LoginResponse> | null = null;

  public readonly currentUser = this.userSignal.asReadonly();
  public readonly accessToken = this.accessTokenSignal.asReadonly();
  public readonly refreshToken = this.refreshTokenSignal.asReadonly();
  public readonly isAuthenticated = computed(() => this.refreshTokenSignal() !== null);

  constructor() {
    this.restoreSession();
  }

  public login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>('/api/auth/login', request)
      .pipe(tap((session) => this.setSession(session)));
  }

  public register(request: RegisterRequest): Observable<User> {
    return this.http.post<User>('/api/auth/register', request);
  }

  public registerAndLogin(request: RegisterRequest): Observable<LoginResponse> {
    return this.register(request).pipe(
      switchMap(() => this.login({ email: request.email, password: request.password })),
    );
  }

  public refresh(): Observable<LoginResponse> {
    if (this.refreshInFlight$) {
      return this.refreshInFlight$;
    }

    const refreshToken = this.refreshTokenSignal();
    if (!refreshToken) {
      return throwError(() => new Error('Missing refresh token'));
    }

    this.refreshInFlight$ = this.http
      .post<LoginResponse>('/api/auth/refresh', { refreshToken })
      .pipe(
        tap((session) => this.setSession(session)),
        finalize(() => {
          this.refreshInFlight$ = null;
        }),
        shareReplay({ bufferSize: 1, refCount: true }),
      );

    return this.refreshInFlight$;
  }

  public logout(): Observable<void> {
    const refreshToken = this.refreshTokenSignal();
    const request$ = refreshToken
      ? this.http.post<void>('/api/auth/logout', { refreshToken })
      : of(undefined);

    return request$.pipe(
      catchError(() => of(undefined)),
      tap(() => this.clearSession()),
    );
  }

  public clearSession(): void {
    this.userSignal.set(null);
    this.accessTokenSignal.set(null);
    this.refreshTokenSignal.set(null);
    sessionStorage.removeItem(AUTH_SESSION_KEY);
  }

  private setSession(session: LoginResponse): void {
    this.accessTokenSignal.set(session.accessToken);
    this.refreshTokenSignal.set(session.refreshToken);
    this.userSignal.set(session.user);
    sessionStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(session));
  }

  private restoreSession(): void {
    const raw = sessionStorage.getItem(AUTH_SESSION_KEY);
    if (!raw) {
      return;
    }

    try {
      const session = JSON.parse(raw) as StoredSession;
      if (!session.accessToken || !session.refreshToken || !session.user) {
        sessionStorage.removeItem(AUTH_SESSION_KEY);
        return;
      }

      this.accessTokenSignal.set(session.accessToken);
      this.refreshTokenSignal.set(session.refreshToken);
      this.userSignal.set(session.user);
    } catch {
      sessionStorage.removeItem(AUTH_SESSION_KEY);
    }
  }
}
