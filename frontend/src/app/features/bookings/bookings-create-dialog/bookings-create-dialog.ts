import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { provideNativeDateAdapter } from '@angular/material/core';
import {
  MatDatepicker,
  MatDatepickerInput,
  MatDatepickerToggle,
} from '@angular/material/datepicker';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { MatError, MatFormField, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { firstValueFrom } from 'rxjs';
import {
  BOOKING_HOURS,
  BOOKING_MAX_DURATION_MS,
  BOOKING_MIN_DURATION_MS,
  bookingDraftFromIso,
  minutesForHour,
  parisTodayDate,
  toUtcIsoFromParisLocal,
  toYmdFromDate,
} from '../bookings.helpers';
import { Booking, BookingsCreateDialogData } from '../bookings.model';
import { BookingsService } from '../bookings.service';

export const BookingCreateErrorMessage = {
  Overlap: 'Ce créneau chevauche une réservation existante pour cette salle.',
  Failed: 'Impossible de créer la réservation.',
  Range: 'La fin doit être après le début.',
  MinDuration: 'La réservation doit durer au moins 15 minutes.',
  MaxDuration: 'La réservation ne peut pas dépasser 12 heures.',
  Past: 'Le début ne peut pas être dans le passé.',
  Invalid: 'Les informations de la réservation sont invalides.',
} as const;

@Component({
  selector: 'app-bookings-create-dialog',
  imports: [
    ReactiveFormsModule,
    MatButton,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatFormField,
    MatLabel,
    MatSelect,
    MatOption,
    MatError,
    MatInput,
    MatDatepicker,
    MatDatepickerInput,
    MatDatepickerToggle,
    MatSuffix,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './bookings-create-dialog.html',
  styleUrl: './bookings-create-dialog.scss',
})
export class BookingsCreateDialog {
  private readonly bookingsService = inject(BookingsService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<BookingsCreateDialog, Booking | undefined>);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly data = inject<BookingsCreateDialogData>(MAT_DIALOG_DATA);
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly hours = BOOKING_HOURS;
  protected readonly minDate = parisTodayDate();
  protected readonly rangeError = BookingCreateErrorMessage.Range;
  protected readonly minDurationError = BookingCreateErrorMessage.MinDuration;
  protected readonly maxDurationError = BookingCreateErrorMessage.MaxDuration;
  protected readonly pastError = BookingCreateErrorMessage.Past;

  protected readonly lockedRoom = this.data.rooms.find((room) => room.id === this.data.roomId);

  private readonly draft = bookingDraftFromIso(this.data.startsAt, this.data.endsAt);

  protected readonly form = this.formBuilder.group(
    {
      date: this.formBuilder.control<Date | null>(this.draft.date, Validators.required),
      startHour: this.formBuilder.nonNullable.control(this.draft.startHour, Validators.required),
      startMinute: this.formBuilder.nonNullable.control(
        this.draft.startMinute,
        Validators.required,
      ),
      endHour: this.formBuilder.nonNullable.control(this.draft.endHour, Validators.required),
      endMinute: this.formBuilder.nonNullable.control(this.draft.endMinute, Validators.required),
      roomId: this.formBuilder.nonNullable.control(this.data.roomId ?? '', Validators.required),
    },
    { validators: [bookingTimesValidator] },
  );

  public constructor() {
    this.form.controls.startHour.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((hour) => {
        if (!this.minutesFor(hour).includes(this.form.controls.startMinute.value)) {
          this.form.controls.startMinute.setValue(0);
        }
      });
    this.form.controls.endHour.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((hour) => {
        if (!this.minutesFor(hour).includes(this.form.controls.endMinute.value)) {
          this.form.controls.endMinute.setValue(0);
        }
      });
  }

  protected minutesFor(hour: number): number[] {
    return minutesForHour(hour);
  }

  protected padTime(value: number): string {
    return String(value).padStart(2, '0');
  }

  protected cancel(): void {
    this.dialogRef.close();
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const { date, startHour, startMinute, endHour, endMinute, roomId } = this.form.getRawValue();
    if (date === null) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    try {
      const ymd = toYmdFromDate(date);
      const booking = await firstValueFrom(
        this.bookingsService.createBooking({
          roomId,
          startsAt: toUtcIsoFromParisLocal(ymd, startHour, startMinute),
          endsAt: toUtcIsoFromParisLocal(ymd, endHour, endMinute),
        }),
      );
      this.dialogRef.close(booking);
    } catch (error: unknown) {
      this.errorMessage.set(this.createErrorMessage(error));
      if (error instanceof HttpErrorResponse && error.status === HttpStatusCode.BadRequest) {
        this.form.markAllAsTouched();
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }

  private createErrorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return BookingCreateErrorMessage.Failed;
    }

    if (error.status === HttpStatusCode.Conflict) {
      return BookingCreateErrorMessage.Overlap;
    }

    if (error.status === HttpStatusCode.BadRequest) {
      return BookingCreateErrorMessage.Invalid;
    }

    return BookingCreateErrorMessage.Failed;
  }
}

function bookingTimesValidator(control: AbstractControl): ValidationErrors | null {
  const date = control.get('date')?.value as Date | null;
  const startHour = Number(control.get('startHour')?.value);
  const startMinute = Number(control.get('startMinute')?.value);
  const endHour = Number(control.get('endHour')?.value);
  const endMinute = Number(control.get('endMinute')?.value);
  if (!(date instanceof Date) || Number.isNaN(date.getTime())) {
    return null;
  }

  const ymd = toYmdFromDate(date);
  const startsAt = Date.parse(toUtcIsoFromParisLocal(ymd, startHour, startMinute));
  const endsAt = Date.parse(toUtcIsoFromParisLocal(ymd, endHour, endMinute));
  if (!(endsAt > startsAt)) {
    return { range: true };
  }

  if (endsAt - startsAt < BOOKING_MIN_DURATION_MS) {
    return { minDuration: true };
  }

  if (endsAt - startsAt > BOOKING_MAX_DURATION_MS) {
    return { maxDuration: true };
  }

  if (startsAt < Date.now()) {
    return { past: true };
  }

  return null;
}
