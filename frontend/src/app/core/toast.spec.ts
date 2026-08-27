import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { vi } from 'vitest';
import { Toast } from './toast';

describe('Toast', () => {
  function setup(): { toast: Toast; snackBar: { open: ReturnType<typeof vi.fn> } } {
    const snackBar = { open: vi.fn() };

    TestBed.configureTestingModule({
      providers: [{ provide: MatSnackBar, useValue: snackBar }],
    });

    return { toast: TestBed.inject(Toast), snackBar };
  }

  it('opens an error snackbar at the top center', () => {
    const { toast, snackBar } = setup();

    toast.error('Impossible de supprimer la salle.');

    expect(snackBar.open).toHaveBeenCalledWith('Impossible de supprimer la salle.', '✕', {
      duration: 4000,
      panelClass: 'snackbar--error',
      verticalPosition: 'top',
      horizontalPosition: 'center',
    });
  });

  it('opens a success snackbar at the top center', () => {
    const { toast, snackBar } = setup();

    toast.success('Salle supprimée.');

    expect(snackBar.open).toHaveBeenCalledWith('Salle supprimée.', '✕', {
      duration: 4000,
      panelClass: 'snackbar--success',
      verticalPosition: 'top',
      horizontalPosition: 'center',
    });
  });
});
