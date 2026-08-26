import { routes } from './app.routes';

describe('routes', () => {
  it('redirects an empty /in child to rooms', () => {
    const inRoute = routes.find((route) => route.path === 'in');
    const emptyChild = inRoute?.children?.find((route) => route.path === '');

    expect(emptyChild?.pathMatch).toBe('full');
    expect(emptyChild?.redirectTo).toBe('rooms');
  });
});
