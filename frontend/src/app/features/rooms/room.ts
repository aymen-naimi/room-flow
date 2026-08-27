export interface Room {
  id: string;
  name: string;
  capacity: number;
  location: string;
  createdAt: string;
}

export interface CreateRoomRequest {
  name: string;
  capacity: number;
  location: string;
}
