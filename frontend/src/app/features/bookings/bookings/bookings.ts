import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormField, MatLabel, MatPrefix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatTooltip } from '@angular/material/tooltip';
import type {
  DateSelectInfo,
  DatesSetInfo,
  EventClickInfo,
  EventInput,
} from '@fullcalendar/angular';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { ConfirmDialog } from '../../../core/confirm-dialog/confirm-dialog';
import { Page } from '../../../core/page/page';
import { Toast } from '../../../core/toast';
import { Room } from '../../rooms/rooms.model';
import { RoomsService } from '../../rooms/rooms.service';
import {
  bookingRangeFromSelection,
  defaultBookingRange,
  roomCapacityLabel,
  toUtcIso,
} from '../bookings.helpers';
import { BookingsCalendar } from '../bookings-calendar/bookings-calendar';
import { BookingsCreateDialog } from '../bookings-create-dialog/bookings-create-dialog';
import { Booking, GetBookingsFilter } from '../bookings.model';
import { BookingsService } from '../bookings.service';

export const BookingsRoomMode = 'room';
export const BookingsMineMode = 'mine';
export const BookingsAgendaTitle = 'Agenda';
export const BookingsDisponibilitesTitle = 'Disponibilités';
export const BookingsAgendaLead = 'Retrouvez vos réservations et réservez un créneau.';
export const BookingsDisponibilitesLead = 'Consultez le planning et réservez un créneau.';
export const BookingsReserveHint = 'Choisissez d’abord une salle';

export type BookingsMode = typeof BookingsRoomMode | typeof BookingsMineMode;

export const BookingDeleteErrorMessage = 'Impossible d’annuler la réservation.';
export const BookingDeleteSuccessMessage = 'Réservation annulée.';
export const BookingDeleteConfirm = 'Annuler la réservation';

@Component({
  selector: 'app-bookings',
  imports: [
    BookingsCalendar,
    Page,
    RouterLink,
    MatButton,
    MatFormField,
    MatIcon,
    MatLabel,
    MatPrefix,
    MatSelect,
    MatOption,
    MatTooltip,
  ],
  templateUrl: './bookings.html',
  styleUrl: './bookings.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Bookings implements OnInit {
  private readonly bookingsService = inject(BookingsService);
  private readonly roomsService = inject(RoomsService);
  private readonly auth = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly mode: BookingsMode =
    (this.route.snapshot.data['mode'] as BookingsMode | undefined) ?? BookingsRoomMode;
  protected readonly isRoomMode = signal(this.mode === BookingsRoomMode);
  protected readonly title = signal(
    this.isRoomMode() ? BookingsDisponibilitesTitle : BookingsAgendaTitle,
  );
  protected readonly lead = signal(
    this.isRoomMode() ? BookingsDisponibilitesLead : BookingsAgendaLead,
  );
  protected readonly rooms = signal<Room[]>([]);
  protected readonly selectedRoomId = signal<string | null>(null);
  protected readonly events = signal<EventInput[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly hasError = signal(false);
  protected readonly isMutating = signal(false);
  protected readonly hasRooms = computed(() => this.rooms().length > 0);
  protected readonly isReserveDisabled = computed(
    () => this.isMutating() || (this.isRoomMode() && !this.selectedRoomId()),
  );
  protected readonly reserveHint = computed(() =>
    this.isRoomMode() && !this.selectedRoomId() ? BookingsReserveHint : '',
  );
  protected readonly roomCapacityLabel = roomCapacityLabel;

  private currentRange: { from: string; to: string } | null = null;

  public async ngOnInit(): Promise<void> {
    try {
      const rooms = await firstValueFrom(this.roomsService.getRooms());
      this.rooms.set(rooms);
      if (this.isRoomMode()) {
        this.applyRoomId(this.route.snapshot.paramMap.get('roomId'));
        this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
          const roomId = params.get('roomId');
          if (roomId === this.selectedRoomId()) {
            return;
          }

          this.applyRoomId(roomId);
          void this.loadBookings();
        });
      }
    } catch {
      this.hasError.set(true);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected onRoomChange(roomId: string): void {
    this.selectedRoomId.set(roomId);
    void this.syncRoomRoute(roomId);
    void this.loadBookings();
  }

  protected openReserveDialog(): void {
    if (!this.hasRooms() || this.isReserveDisabled()) {
      return;
    }

    const range = defaultBookingRange();
    void this.openCreateDialog(range.startsAt, range.endsAt);
  }

  private applyRoomId(roomId: string | null): void {
    const match = roomId ? this.rooms().find((room) => room.id === roomId) : undefined;
    this.selectedRoomId.set(match?.id ?? null);
    if (!match) {
      this.events.set([]);
    }
  }

  private async syncRoomRoute(roomId: string): Promise<void> {
    await this.router.navigate(['/in/bookings', roomId], { replaceUrl: true });
  }

  protected async onDatesSet(info: DatesSetInfo): Promise<void> {
    this.currentRange = {
      from: toUtcIso(info.start),
      to: toUtcIso(info.end),
    };
    await this.loadBookings();
  }

  private async loadBookings(): Promise<void> {
    if (!this.currentRange) {
      return;
    }

    const filter = this.bookingsFilter();
    if (this.isRoomMode() && !filter.roomId) {
      return;
    }

    try {
      const bookings = await firstValueFrom(
        this.bookingsService.getBookings(this.currentRange.from, this.currentRange.to, filter),
      );
      this.events.set(bookings.map((booking) => this.toEvent(booking)));
      this.hasError.set(false);
    } catch {
      this.hasError.set(true);
    }
  }

  private bookingsFilter(): GetBookingsFilter {
    if (this.isRoomMode()) {
      return { roomId: this.selectedRoomId() ?? undefined };
    }

    return { mine: true };
  }

  protected async onSelect(info: DateSelectInfo): Promise<void> {
    info.view.calendar.unselect();
    if (!this.hasRooms() || this.isMutating()) {
      return;
    }

    const range = bookingRangeFromSelection(info.start, info.end);
    await this.openCreateDialog(range.startsAt, range.endsAt);
  }

  private async openCreateDialog(startsAt: string, endsAt: string): Promise<void> {
    const created = await firstValueFrom(
      this.dialog
        .open(BookingsCreateDialog, {
          data: {
            rooms: this.rooms(),
            startsAt,
            endsAt,
            roomId: this.isRoomMode() ? (this.selectedRoomId() ?? undefined) : undefined,
          },
        })
        .afterClosed(),
    );

    if (created) {
      await this.loadBookings();
    }
  }

  protected async onEventClick(info: EventClickInfo): Promise<void> {
    const bookingId = String(info.event.id);
    const userId = String(info.event.extendedProps['userId'] ?? '');
    const currentUserId = this.auth.currentUser()?.id;
    if (!currentUserId || userId !== currentUserId || this.isMutating()) {
      return;
    }

    this.isMutating.set(true);
    try {
      const confirmed = await firstValueFrom(
        this.dialog
          .open(ConfirmDialog, {
            data: {
              title: BookingDeleteConfirm,
              message: `Annuler « ${info.event.title} » ?`,
              confirmLabel: BookingDeleteConfirm,
              destructive: true,
            },
          })
          .afterClosed(),
      );
      if (!confirmed) {
        return;
      }

      await firstValueFrom(this.bookingsService.deleteBooking(bookingId));
      this.toast.success(BookingDeleteSuccessMessage);
      await this.loadBookings();
    } catch {
      this.toast.error(BookingDeleteErrorMessage);
    } finally {
      this.isMutating.set(false);
    }
  }

  private toEvent(booking: Booking): EventInput {
    return {
      id: booking.id,
      title: this.isRoomMode()
        ? booking.userDisplayName
        : roomCapacityLabel(booking.roomName, this.roomCapacity(booking.roomId)),
      start: booking.startsAt,
      end: booking.endsAt,
      extendedProps: {
        userId: booking.userId,
        roomId: booking.roomId,
      },
    };
  }

  private roomCapacity(roomId: string): number | undefined {
    return this.rooms().find((room) => room.id === roomId)?.capacity;
  }
}
