import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { AuthErrorMessage } from './auth-error.enums';

export function authErrorMessage(error: unknown, fallback: AuthErrorMessage): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  switch (error.status) {
    case HttpStatusCode.TooManyRequests:
      return AuthErrorMessage.TooManyAttempts;
    case HttpStatusCode.Unauthorized:
      return AuthErrorMessage.InvalidCredentials;
    case HttpStatusCode.Conflict:
      return AuthErrorMessage.EmailTaken;
    default: {
      const detail = (error.error as { detail?: unknown } | null)?.detail;
      if (typeof detail === 'string' && detail.length > 0) {
        return detail;
      }

      return fallback;
    }
  }
}
