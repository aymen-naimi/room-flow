import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthErrorMessage } from '../../../core/auth/auth-error.enums';
import { loginResponseMock } from '../../../core/auth/auth.mock';
import { Register } from './register';

describe('Register', () => {
  afterEach(() => {
    sessionStorage.clear();
  });

  async function setup(): Promise<{
    fixture: ComponentFixture<Register>;
    http: HttpTestingController;
  }> {
    sessionStorage.clear();
    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(Register);
    fixture.detectChanges();

    return { fixture, http: TestBed.inject(HttpTestingController) };
  }

  function fillAndSubmit(fixture: ComponentFixture<Register>): void {
    setInput(fixture, 'firstName', 'Jane');
    setInput(fixture, 'lastName', 'Doe');
    setInput(fixture, 'email', 'jane.doe@example.com');
    setInput(fixture, 'password', 'password1');
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  function setInput(fixture: ComponentFixture<Register>, name: string, value: string): void {
    const input = fixture.nativeElement.querySelector(
      `input[formControlName="${name}"]`,
    ) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  it('creates the user then logs in', async () => {
    const { fixture, http } = await setup();
    fillAndSubmit(fixture);

    http.expectOne('/api/auth/register').flush(loginResponseMock.user);
    http.expectOne('/api/auth/login').flush(loginResponseMock);
    await fixture.whenStable();

    expect(sessionStorage.getItem('room-flow.session')).toContain(loginResponseMock.refreshToken);
    http.verify();
  });

  it('shows email taken on 409', async () => {
    const { fixture, http } = await setup();
    fillAndSubmit(fixture);

    http.expectOne('/api/auth/register').flush(null, { status: 409, statusText: 'Conflict' });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.auth__error').textContent).toContain(
      AuthErrorMessage.EmailTaken,
    );
    http.verify();
  });
});
