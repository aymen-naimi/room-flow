import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { ConfirmDialog } from '../../../core/confirm-dialog/confirm-dialog';
import { Page } from '../../../core/page/page';
import { Toast } from '../../../core/toast';
import { Room } from '../rooms.model';
import { RoomsService } from '../rooms.service';

export const RoomDeleteErrorMessage = 'Impossible de supprimer la salle.';
export const RoomDeleteSuccessMessage = 'Salle supprimée.';

@Component({
  selector: 'app-rooms-list',
  imports: [DatePipe, Page, RouterLink, MatButton, MatIconButton, MatIcon, MatTooltip],
  templateUrl: './rooms-list.html',
  styleUrl: './rooms-list.scss',
})
export class RoomsList implements OnInit {
  private readonly roomsService = inject(RoomsService);
  private readonly dialog = inject(MatDialog);
  private readonly toast = inject(Toast);
  private readonly auth = inject(AuthService);

  protected readonly isAdmin = this.auth.isAdmin;

  protected readonly rooms = signal<Room[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly hasError = signal(false);
  protected readonly hasRooms = computed(() => this.rooms().length > 0);
  protected readonly isDeleting = signal(false);

  public async ngOnInit(): Promise<void> {
    await this.getRooms();
  }

  protected async onDelete(room: Room): Promise<void> {
    if (this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);

    try {
      const confirmed = await firstValueFrom(
        this.dialog
          .open(ConfirmDialog, {
            data: {
              title: 'Supprimer la salle',
              message: `Supprimer la salle « ${room.name} » ?`,
              confirmLabel: 'Supprimer',
              destructive: true,
            },
          })
          .afterClosed(),
      );
      if (!confirmed) {
        return;
      }

      await firstValueFrom(this.roomsService.deleteRoom(room.id));
      this.toast.success(RoomDeleteSuccessMessage);
      await this.getRooms();
    } catch {
      this.toast.error(RoomDeleteErrorMessage);
    } finally {
      this.isDeleting.set(false);
    }
  }

  private async getRooms(): Promise<void> {
    try {
      const rooms = await firstValueFrom(this.roomsService.getRooms());
      this.rooms.set(rooms);
    } catch {
      this.hasError.set(true);
    } finally {
      this.isLoading.set(false);
    }
  }
}
