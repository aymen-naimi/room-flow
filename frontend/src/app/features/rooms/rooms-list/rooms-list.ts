import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Room } from '../room';
import { RoomsService } from '../rooms.service';

@Component({
  selector: 'app-rooms-list',
  imports: [DatePipe],
  templateUrl: './rooms-list.html',
  styleUrl: './rooms-list.scss',
})
export class RoomsList implements OnInit {
  private readonly roomsService = inject(RoomsService);

  protected readonly rooms = signal<Room[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly hasError = signal(false);
  protected readonly hasRooms = computed(() => this.rooms().length > 0);

  public async ngOnInit(): Promise<void> {
    await this.getRooms();
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
