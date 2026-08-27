import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';
import { ConfirmDialog, ConfirmDialogData } from './confirm-dialog';

describe('ConfirmDialog', () => {
  async function setup(data: ConfirmDialogData): Promise<{
    nativeElement: HTMLElement;
    close: ReturnType<typeof vi.fn>;
  }> {
    const close = vi.fn();

    await TestBed.configureTestingModule({
      imports: [ConfirmDialog],
      providers: [
        { provide: MatDialogRef, useValue: { close } },
        { provide: MAT_DIALOG_DATA, useValue: data },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(ConfirmDialog);
    fixture.detectChanges();

    return { nativeElement: fixture.nativeElement, close };
  }

  it('styles the confirm button as destructive when requested', async () => {
    const { nativeElement } = await setup({
      title: 'Supprimer la salle',
      message: 'Supprimer la salle « Horizon » ?',
      confirmLabel: 'Supprimer',
      destructive: true,
    });
    const confirm = nativeElement.querySelector('.confirm-dialog__confirm');

    expect(confirm?.textContent).toContain('Supprimer');
    expect(confirm?.classList.contains('confirm-dialog__confirm--destructive')).toBe(true);
  });

  it('keeps the confirm button neutral without destructive', async () => {
    const { nativeElement } = await setup({
      title: 'Confirmer',
      message: 'Continuer ?',
      confirmLabel: 'Continuer',
    });
    const confirm = nativeElement.querySelector('.confirm-dialog__confirm');

    expect(confirm?.classList.contains('confirm-dialog__confirm--destructive')).toBe(false);
  });
});
