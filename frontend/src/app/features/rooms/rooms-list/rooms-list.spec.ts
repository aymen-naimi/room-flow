import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { Toast } from '../../../core/toast';
import { ConfirmDialog } from '../../../core/confirm-dialog/confirm-dialog';
import { Room } from '../room';
import { roomHorizonMock, roomsMock } from '../rooms.mock';
import { RoomDeleteErrorMessage, RoomDeleteSuccessMessage, RoomsList } from './rooms-list';

const roomNordMock: Room = {
  ...roomHorizonMock,
  id: '22222222-2222-2222-2222-222222222222',
  name: 'Salle Nord',
};

describe('RoomsList', () => {
  async function setup(
    confirmed = true,
    rooms: Room[] = roomsMock,
  ): Promise<{
    fixture: ReturnType<typeof TestBed.createComponent<RoomsList>>;
    http: HttpTestingController;
    toast: { error: ReturnType<typeof vi.fn>; success: ReturnType<typeof vi.fn> };
    openDialog: ReturnType<typeof vi.fn>;
  }> {
    const toast = { error: vi.fn(), success: vi.fn() };
    const openDialog = vi.fn(() => ({ afterClosed: () => of(confirmed) }));

    await TestBed.configureTestingModule({
      imports: [RoomsList],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: MatDialog,
          useValue: {
            open: openDialog,
          },
        },
        { provide: Toast, useValue: toast },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(RoomsList);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('/api/rooms').flush(rooms);
    await fixture.whenStable();
    fixture.detectChanges();

    return { fixture, http, toast, openDialog };
  }

  function deleteButtons(fixture: ReturnType<typeof TestBed.createComponent<RoomsList>>): HTMLButtonElement[] {
    return [...fixture.nativeElement.querySelectorAll('.rooms__delete')];
  }

  it('displays rooms returned by the API', async () => {
    const { fixture, http } = await setup();

    expect(fixture.nativeElement.textContent).toContain(roomsMock[0].name);
    expect(fixture.nativeElement.textContent).toContain(roomsMock[0].capacity);
    expect(fixture.nativeElement.textContent).toContain(roomsMock[0].location);
    expect(fixture.nativeElement.querySelector('.rooms__add').getAttribute('href')).toBe(
      '/in/rooms/new',
    );
    expect(fixture.nativeElement.querySelector('.rooms__delete').getAttribute('aria-label')).toBe(
      'Supprimer',
    );
    expect(fixture.nativeElement.querySelector('.rooms__delete').getAttribute('mattooltip')).toBe(
      'Supprimer',
    );
    http.verify();
  });

  it('deletes a room after confirmation then reloads the list', async () => {
    const { fixture, http, openDialog, toast } = await setup();
    fixture.nativeElement.querySelector('.rooms__delete').click();
    await fixture.whenStable();

    expect(openDialog).toHaveBeenCalledWith(
      ConfirmDialog,
      expect.objectContaining({
        data: expect.objectContaining({
          confirmLabel: 'Supprimer',
          destructive: true,
        }),
      }),
    );

    const request = http.expectOne(`/api/rooms/${roomHorizonMock.id}`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null, { status: 204, statusText: 'No Content' });
    await fixture.whenStable();

    http.expectOne('/api/rooms').flush([]);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain(roomHorizonMock.name);
    expect(fixture.nativeElement.textContent).toContain('Aucune salle pour le moment.');
    expect(toast.success).toHaveBeenCalledWith(RoomDeleteSuccessMessage);
    http.verify();
  });

  it('disables all delete buttons while a deletion is in progress', async () => {
    const { fixture, http } = await setup(true, [roomHorizonMock, roomNordMock]);
    deleteButtons(fixture)[0].click();
    await fixture.whenStable();
    fixture.detectChanges();

    const request = http.expectOne(`/api/rooms/${roomHorizonMock.id}`);
    expect(deleteButtons(fixture)).toHaveLength(2);
    expect(deleteButtons(fixture).every((button) => button.disabled)).toBe(true);

    request.flush(null, { status: 204, statusText: 'No Content' });
    await fixture.whenStable();
    http.expectOne('/api/rooms').flush([roomNordMock]);
    await fixture.whenStable();
    await Promise.resolve();
    fixture.detectChanges();

    expect(deleteButtons(fixture)).toHaveLength(1);
    expect(deleteButtons(fixture)[0].disabled).toBe(false);
    http.verify();
  });

  it('shows an error and keeps the room when delete fails', async () => {
    const { fixture, http, toast } = await setup();
    fixture.nativeElement.querySelector('.rooms__delete').click();
    await fixture.whenStable();

    http.expectOne(`/api/rooms/${roomHorizonMock.id}`).flush(null, {
      status: 404,
      statusText: 'Not Found',
    });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(roomHorizonMock.name);
    expect(toast.error).toHaveBeenCalledWith(RoomDeleteErrorMessage);
    expect(toast.success).not.toHaveBeenCalled();
    http.verify();
  });

  it('does not delete when confirmation is cancelled', async () => {
    const { fixture, http, toast } = await setup(false);
    fixture.nativeElement.querySelector('.rooms__delete').click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(deleteButtons(fixture)[0].disabled).toBe(false);
    expect(toast.success).not.toHaveBeenCalled();
    http.verify();
  });
});
