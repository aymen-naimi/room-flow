import { inject, Service } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Service()
export class Toast {
  private readonly snackBar = inject(MatSnackBar);

  public error(message: string): void {
    this.show(message, 'snackbar--error');
  }

  public success(message: string): void {
    this.show(message, 'snackbar--success');
  }

  public info(message: string): void {
    this.show(message, 'snackbar--info');
  }

  private show(message: string, panelClass: string): void {
    this.snackBar.open(message, '✕', {
      duration: 4000,
      panelClass,
      verticalPosition: 'top',
      horizontalPosition: 'center',
    });
  }
}
