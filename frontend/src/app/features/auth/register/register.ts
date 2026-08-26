import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { firstValueFrom } from 'rxjs';
import { authErrorMessage } from '../../../core/auth/auth-error';
import { AuthErrorMessage } from '../../../core/auth/auth-error.enums';
import { AuthService } from '../../../core/auth/auth.service';
import { AuthLayout } from '../auth-layout/auth-layout';

@Component({
  selector: 'app-register',
  imports: [
    AuthLayout,
    ReactiveFormsModule,
    RouterLink,
    MatButton,
    MatFormField,
    MatLabel,
    MatInput,
    MatError,
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private readonly auth = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(100)]],
  });

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    try {
      await firstValueFrom(this.auth.registerAndLogin(this.form.getRawValue()));
      await this.router.navigateByUrl('/in/rooms');
    } catch (error: unknown) {
      this.errorMessage.set(authErrorMessage(error, AuthErrorMessage.RegisterFailed));
      if (error instanceof HttpErrorResponse && error.status === HttpStatusCode.BadRequest) {
        this.form.markAllAsTouched();
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
