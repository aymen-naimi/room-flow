import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, MatButton],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
})
export class MainLayout {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly user = this.auth.currentUser;

  protected async logout(): Promise<void> {
    await firstValueFrom(this.auth.logout());
    await this.router.navigateByUrl('/login');
  }
}
