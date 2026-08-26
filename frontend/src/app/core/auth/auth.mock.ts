import { LoginResponse } from './auth.models';

export const loginResponseMock: LoginResponse = {
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  user: {
    id: '11111111-1111-1111-1111-111111111111',
    email: 'jane.doe@example.com',
    firstName: 'Jane',
    lastName: 'Doe',
  },
};
