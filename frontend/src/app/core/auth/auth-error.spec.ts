import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { authErrorMessage } from './auth-error';
import { AuthErrorMessage } from './auth-error.enums';

describe('authErrorMessage', () => {
  function httpError(status: HttpStatusCode): HttpErrorResponse {
    return new HttpErrorResponse({ status, statusText: String(status) });
  }

  it('maps 401 to invalid credentials', () => {
    expect(authErrorMessage(httpError(HttpStatusCode.Unauthorized), AuthErrorMessage.LoginFailed)).toBe(
      AuthErrorMessage.InvalidCredentials,
    );
  });

  it('maps 409 to email taken', () => {
    expect(authErrorMessage(httpError(HttpStatusCode.Conflict), AuthErrorMessage.RegisterFailed)).toBe(
      AuthErrorMessage.EmailTaken,
    );
  });

  it('maps 429 to too many attempts', () => {
    expect(
      authErrorMessage(httpError(HttpStatusCode.TooManyRequests), AuthErrorMessage.LoginFailed),
    ).toBe(AuthErrorMessage.TooManyAttempts);
  });
});
