import {
  HttpContextToken,
  HttpErrorResponse,
  HttpInterceptorFn,
  HttpRequest,
  HttpStatusCode,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

const AUTH_RETRY = new HttpContextToken(() => false);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const request = withAccessToken(req, auth.accessToken());

  return next(request).pipe(
    catchError((error: unknown) => {
      if (!shouldRefresh(error, request, auth.refreshToken())) {
        return throwError(() => error);
      }

      return auth.refresh().pipe(
        switchMap(() => {
          const retried = withAccessToken(
            request.clone({ context: request.context.set(AUTH_RETRY, true) }),
            auth.accessToken(),
          );
          return next(retried);
        }),
        catchError((refreshError: unknown) => {
          auth.clearSession();
          void router.navigateByUrl('/login');
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};

function shouldRefresh(
  error: unknown,
  request: HttpRequest<unknown>,
  refreshToken: string | null,
): boolean {
  return (
    error instanceof HttpErrorResponse &&
    error.status === HttpStatusCode.Unauthorized &&
    !isAnonymousAuthUrl(request.url) &&
    !request.context.get(AUTH_RETRY) &&
    refreshToken !== null
  );
}

function withAccessToken(
  req: HttpRequest<unknown>,
  accessToken: string | null,
): HttpRequest<unknown> {
  if (!accessToken || isAnonymousAuthUrl(req.url)) {
    return req;
  }

  return req.clone({
    setHeaders: { Authorization: `Bearer ${accessToken}` },
  });
}

function isAnonymousAuthUrl(url: string): boolean {
  return (
    url.includes('/api/auth/login') ||
    url.includes('/api/auth/register') ||
    url.includes('/api/auth/refresh') ||
    url.includes('/api/auth/logout')
  );
}
