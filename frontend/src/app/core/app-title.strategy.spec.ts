import { TestBed } from '@angular/core/testing';
import { Title } from '@angular/platform-browser';
import { RouterStateSnapshot } from '@angular/router';
import { vi } from 'vitest';
import { AppTitle, AppTitleStrategy } from './app-title.strategy';

describe('AppTitleStrategy', () => {
  function setup(): { strategy: AppTitleStrategy; setTitle: ReturnType<typeof vi.fn> } {
    const setTitle = vi.fn();
    TestBed.configureTestingModule({
      providers: [AppTitleStrategy, { provide: Title, useValue: { setTitle } }],
    });

    return { strategy: TestBed.inject(AppTitleStrategy), setTitle };
  }

  it('suffixes the page title with the app name', () => {
    const { strategy, setTitle } = setup();
    vi.spyOn(strategy, 'buildTitle').mockReturnValue('Agenda');

    strategy.updateTitle({} as RouterStateSnapshot);

    expect(setTitle).toHaveBeenCalledWith(`Agenda · ${AppTitle}`);
  });

  it('uses the app name when the route has no title', () => {
    const { strategy, setTitle } = setup();
    vi.spyOn(strategy, 'buildTitle').mockReturnValue(undefined);

    strategy.updateTitle({} as RouterStateSnapshot);

    expect(setTitle).toHaveBeenCalledWith(AppTitle);
  });
});
