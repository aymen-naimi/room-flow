import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { firstValueFrom } from 'rxjs';
import { FormValidators } from '../../../core/form-validators';
import { RoomsService } from '../rooms.service';

export const RoomCreateErrorMessage = {
  NameTaken: 'Une salle avec ce nom existe déjà.',
  Failed: 'Impossible de créer la salle.',
} as const;

@Component({
  selector: 'app-rooms-create',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButton,
    MatFormField,
    MatLabel,
    MatInput,
    MatError,
  ],
  templateUrl: './rooms-create.html',
  styleUrl: './rooms-create.scss',
})
export class RoomsCreate {
  private readonly roomsService = inject(RoomsService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.formBuilder.group({
    name: this.formBuilder.nonNullable.control('', [
      FormValidators.requiredText,
      Validators.maxLength(200),
    ]),
    capacity: this.formBuilder.control<number | null>(null, [
      Validators.required,
      FormValidators.integer,
      Validators.min(1),
      Validators.max(1000),
    ]),
    location: this.formBuilder.nonNullable.control('', [
      FormValidators.requiredText,
      Validators.maxLength(200),
    ]),
  });

  protected async onSubmit(): Promise<void> {
    const { name, capacity, location } = this.form.getRawValue();
    const trimmedName = name.trim();
    const trimmedLocation = location.trim();
    if (this.form.invalid || this.isSubmitting() || capacity === null) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    try {
      await firstValueFrom(
        this.roomsService.createRoom({
          name: trimmedName,
          capacity,
          location: trimmedLocation,
        }),
      );
      await this.router.navigateByUrl('/in/rooms');
    } catch (error: unknown) {
      this.errorMessage.set(this.roomCreateErrorMessage(error));
      if (error instanceof HttpErrorResponse && error.status === HttpStatusCode.BadRequest) {
        this.form.markAllAsTouched();
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }

  private roomCreateErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === HttpStatusCode.Conflict) {
      return RoomCreateErrorMessage.NameTaken;
    }

    return RoomCreateErrorMessage.Failed;
  }
}
