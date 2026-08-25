import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { RoomsList } from './rooms-list';
import { roomsMock } from '../rooms.mock';

describe('RoomsList', () => {
  it('displays rooms returned by the API', async () => {
    await TestBed.configureTestingModule({
      imports: [RoomsList],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(RoomsList);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();

    http.expectOne('/api/rooms').flush(roomsMock);

    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(roomsMock[0].name);
    expect(fixture.nativeElement.textContent).toContain(roomsMock[0].capacity);
    expect(fixture.nativeElement.textContent).toContain(roomsMock[0].location);
    http.verify();
  });
});
