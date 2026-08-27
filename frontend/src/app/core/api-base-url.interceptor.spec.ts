import { withApiBaseUrl } from './api-base-url.interceptor';

describe('withApiBaseUrl', () => {
  it('keeps relative /api URLs when the base is empty', () => {
    expect(withApiBaseUrl('/api/rooms', '')).toBe('/api/rooms');
  });

  it('prefixes relative /api URLs with the Azure API origin', () => {
    expect(withApiBaseUrl('/api/auth/login', 'https://api.example.com')).toBe(
      'https://api.example.com/api/auth/login',
    );
  });

  it('strips a trailing slash on the base URL', () => {
    expect(withApiBaseUrl('/api/rooms', 'https://api.example.com/')).toBe(
      'https://api.example.com/api/rooms',
    );
  });

  it('does not rewrite absolute URLs', () => {
    expect(withApiBaseUrl('https://other.example.com/api/rooms', 'https://api.example.com')).toBe(
      'https://other.example.com/api/rooms',
    );
  });
});
