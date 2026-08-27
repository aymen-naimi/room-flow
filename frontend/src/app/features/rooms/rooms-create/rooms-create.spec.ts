import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { vi } from 'vitest';
import { createRoomRequestMock, roomHorizonMock } from '../rooms.mock';
import { RoomCreateErrorMessage, RoomsCreate } from './rooms-create';

describe('RoomsCreate', () => {
  async function setup(): Promise<{
    fixture: ComponentFixture<RoomsCreate>;
    http: HttpTestingController;
  }> {
    await TestBed.configureTestingModule({
      imports: [RoomsCreate],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(RoomsCreate);
    fixture.detectChanges();

    return { fixture, http: TestBed.inject(HttpTestingController) };
  }

  function fillAndSubmit(fixture: ComponentFixture<RoomsCreate>): void {
    setInput(fixture, 'name', createRoomRequestMock.name);
    setInput(fixture, 'capacity', String(createRoomRequestMock.capacity));
    setInput(fixture, 'location', createRoomRequestMock.location);
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }

  function setInput(fixture: ComponentFixture<RoomsCreate>, name: string, value: string): void {
    const input = fixture.nativeElement.querySelector(
      `input[formControlName="${name}"]`,
    ) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  it('does not submit an empty form', async () => {
    const { fixture, http } = await setup();
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    http.expectNone('/api/rooms');
    expect(fixture.nativeElement.textContent).toContain('Le nom est requis.');
    expect(fixture.nativeElement.textContent).toContain('La capacité est requise.');
    expect(fixture.nativeElement.textContent).toContain("L'emplacement est requis.");
    http.verify();
  });

  it('does not submit a decimal capacity', async () => {
    const { fixture, http } = await setup();
    setInput(fixture, 'name', createRoomRequestMock.name);
    setInput(fixture, 'capacity', '1.5');
    setInput(fixture, 'location', createRoomRequestMock.location);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.rooms-create__submit').disabled).toBe(true);

    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    http.expectNone('/api/rooms');
    expect(fixture.nativeElement.textContent).toContain('Un nombre entier est requis.');
    http.verify();
  });

  it('does not submit whitespace-only name or location', async () => {
    const { fixture, http } = await setup();
    setInput(fixture, 'name', '   ');
    setInput(fixture, 'capacity', String(createRoomRequestMock.capacity));
    setInput(fixture, 'location', '   ');
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.rooms-create__submit').disabled).toBe(true);

    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    http.expectNone('/api/rooms');
    expect(fixture.nativeElement.textContent).toContain('Le nom est requis.');
    expect(fixture.nativeElement.textContent).toContain("L'emplacement est requis.");
    http.verify();
  });

  it('trims name and location before creating', async () => {
    const { fixture, http } = await setup();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    setInput(fixture, 'name', `  ${createRoomRequestMock.name}  `);
    setInput(fixture, 'capacity', String(createRoomRequestMock.capacity));
    setInput(fixture, 'location', `  ${createRoomRequestMock.location}  `);
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    const request = http.expectOne('/api/rooms');
    expect(request.request.body).toEqual(createRoomRequestMock);
    request.flush(roomHorizonMock);

    await vi.waitFor(() => {
      expect(navigate).toHaveBeenCalledWith('/in/rooms');
    });
    http.verify();
  });

  it('creates the room then navigates to the list', async () => {
    const { fixture, http } = await setup();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    fillAndSubmit(fixture);

    const request = http.expectOne('/api/rooms');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(createRoomRequestMock);
    request.flush(roomHorizonMock);

    await vi.waitFor(() => {
      expect(navigate).toHaveBeenCalledWith('/in/rooms');
    });
    http.verify();
  });

  it('shows name taken on 409', async () => {
    const { fixture, http } = await setup();
    fillAndSubmit(fixture);

    http.expectOne('/api/rooms').flush(null, { status: 409, statusText: 'Conflict' });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.rooms-create__error').textContent).toContain(
      RoomCreateErrorMessage.NameTaken,
    );
    http.verify();
  });
});
