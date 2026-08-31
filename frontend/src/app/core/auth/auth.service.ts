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
} from 'rxjs';
import { LoginRequest, LoginResponse, RegisterRequest, User } from './auth.models';

export const AUTH_SESSION_KEY = 'room-flow.session';

interface StoredSession {
  user: User;
}

@Service()
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly userSignal = signal<User | null>(null);
  private readonly accessTokenSignal = signal<string | null>(null);
  private refreshInFlight$: Observable<LoginResponse> | null = null;

  public readonly currentUser = this.userSignal.asReadonly();
  public readonly accessToken = this.accessTokenSignal.asReadonly();
  public readonly isAuthenticated = computed(() => this.userSignal() !== null);
  public readonly isAdmin = computed(() => this.userSignal()?.role === 'Admin');

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

    this.refreshInFlight$ = this.http.post<LoginResponse>('/api/auth/refresh', {}).pipe(
      tap((session) => this.setSession(session)),
      finalize(() => {
        this.refreshInFlight$ = null;
      }),
      shareReplay({ bufferSize: 1, refCount: true }),
    );

    return this.refreshInFlight$;
  }

  public logout(): Observable<void> {
    const request$ = this.isAuthenticated()
      ? this.http.post<void>('/api/auth/logout', {})
      : of(undefined);

    return request$.pipe(
      catchError(() => of(undefined)),
      tap(() => this.clearSession()),
    );
  }

  public clearSession(): void {
    this.userSignal.set(null);
    this.accessTokenSignal.set(null);
    sessionStorage.removeItem(AUTH_SESSION_KEY);
  }

  private setSession(session: LoginResponse): void {
    this.accessTokenSignal.set(session.accessToken);
    this.userSignal.set(session.user);
    sessionStorage.setItem(AUTH_SESSION_KEY, JSON.stringify({ user: session.user } satisfies StoredSession));
  }

  private restoreSession(): void {
    const raw = sessionStorage.getItem(AUTH_SESSION_KEY);
    if (!raw) {
      return;
    }

    try {
      const session = JSON.parse(raw) as StoredSession;
      if (!session.user) {
        sessionStorage.removeItem(AUTH_SESSION_KEY);
        return;
      }

      this.userSignal.set(session.user);
      sessionStorage.setItem(AUTH_SESSION_KEY, JSON.stringify({ user: session.user } satisfies StoredSession));
    } catch {
      sessionStorage.removeItem(AUTH_SESSION_KEY);
    }
  }
}
