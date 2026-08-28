import { CreateRoomRequest, Room } from './rooms.model';

export const roomHorizonMock: Room = {
  id: '11111111-1111-1111-1111-111111111111',
  name: 'Salle Horizon',
  capacity: 8,
  location: 'Étage 2',
  createdAt: '2026-08-25T10:00:00Z',
};

export const createRoomRequestMock: CreateRoomRequest = {
  name: roomHorizonMock.name,
  capacity: roomHorizonMock.capacity,
  location: roomHorizonMock.location,
};

export const roomsMock: Room[] = [roomHorizonMock];
