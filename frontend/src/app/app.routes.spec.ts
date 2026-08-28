import { routes } from './app.routes';

describe('routes', () => {
  it('redirects an empty /in child to my-bookings', () => {
    const inRoute = routes.find((route) => route.path === 'in');
    const emptyChild = inRoute?.children?.find((route) => route.path === '');

    expect(emptyChild?.pathMatch).toBe('full');
    expect(emptyChild?.redirectTo).toBe('my-bookings');
  });

  it('declares bookings under /in as room mode', () => {
    const inRoute = routes.find((route) => route.path === 'in');
    const bookingsChild = inRoute?.children?.find((route) => route.path === 'bookings');

    expect(bookingsChild).toBeDefined();
    expect(bookingsChild?.data).toEqual({ mode: 'room' });
  });

  it('declares bookings/:roomId under /in as room mode', () => {
    const inRoute = routes.find((route) => route.path === 'in');
    const roomBookingsChild = inRoute?.children?.find((route) => route.path === 'bookings/:roomId');

    expect(roomBookingsChild).toBeDefined();
    expect(roomBookingsChild?.data).toEqual({ mode: 'room' });
  });

  it('declares my-bookings under /in as mine mode', () => {
    const inRoute = routes.find((route) => route.path === 'in');
    const myBookingsChild = inRoute?.children?.find((route) => route.path === 'my-bookings');

    expect(myBookingsChild).toBeDefined();
    expect(myBookingsChild?.data).toEqual({ mode: 'mine' });
  });

  it('declares rooms/new under /in', () => {
    const inRoute = routes.find((route) => route.path === 'in');
    const createChild = inRoute?.children?.find((route) => route.path === 'rooms/new');

    expect(createChild).toBeDefined();
  });
});
