import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { createRoomRequestMock, roomHorizonMock, roomsMock } from './rooms.mock';
import { RoomsService } from './rooms.service';

describe('RoomsService', () => {
  async function setup(): Promise<{ rooms: RoomsService; http: HttpTestingController }> {
    await TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    return {
      rooms: TestBed.inject(RoomsService),
      http: TestBed.inject(HttpTestingController),
    };
  }

  it('loads rooms from the API', async () => {
    const { rooms, http } = await setup();
    let result = roomsMock;

    rooms.getRooms().subscribe((value) => {
      result = value;
    });
    http.expectOne('/api/rooms').flush(roomsMock);

    expect(result).toEqual(roomsMock);
    http.verify();
  });

  it('creates a room with the given payload', async () => {
    const { rooms, http } = await setup();
    let result = roomHorizonMock;

    rooms.createRoom(createRoomRequestMock).subscribe((value) => {
      result = value;
    });

    const request = http.expectOne('/api/rooms');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(createRoomRequestMock);
    request.flush(roomHorizonMock);

    expect(result).toEqual(roomHorizonMock);
    http.verify();
  });
});
