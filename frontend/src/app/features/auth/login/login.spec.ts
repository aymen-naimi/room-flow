import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthErrorMessage } from '../../../core/auth/auth-error.enums';
import { loginResponseMock } from '../../../core/auth/auth.mock';
import { Login } from './login';

describe('Login', () => {
  afterEach(() => {
    sessionStorage.clear();
  });

  async function setup(): Promise<{
    fixture: ComponentFixture<Login>;
    http: HttpTestingController;
  }> {
    sessionStorage.clear();
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(Login);
    fixture.detectChanges();

    return { fixture, http: TestBed.inject(HttpTestingController) };
  }

  function fillAndSubmit(fixture: ComponentFixture<Login>): void {
    setInput(fixture, 'email', 'jane.doe@example.com');
    setInput(fixture, 'password', 'password1');
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  function setInput(fixture: ComponentFixture<Login>, name: string, value: string): void {
    const input = fixture.nativeElement.querySelector(
      `input[formControlName="${name}"]`,
    ) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  it('logs in and stores the session', async () => {
    const { fixture, http } = await setup();
    fillAndSubmit(fixture);

    http.expectOne('/api/auth/login').flush(loginResponseMock);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(sessionStorage.getItem('room-flow.session')).toContain(loginResponseMock.user.email);
    expect(sessionStorage.getItem('room-flow.session')).not.toContain(loginResponseMock.accessToken);
    http.verify();
  });

  it('shows invalid credentials on 401', async () => {
    const { fixture, http } = await setup();
    fillAndSubmit(fixture);

    http.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.auth__error').textContent).toContain(
      AuthErrorMessage.InvalidCredentials,
    );
    http.verify();
  });
});
