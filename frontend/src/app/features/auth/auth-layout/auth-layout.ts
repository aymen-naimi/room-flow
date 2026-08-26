import { Component, input } from '@angular/core';
import { MatCard, MatCardContent, MatCardHeader, MatCardTitle } from '@angular/material/card';

@Component({
  selector: 'app-auth-layout',
  imports: [MatCard, MatCardHeader, MatCardTitle, MatCardContent],
  templateUrl: './auth-layout.html',
  styleUrl: './auth-layout.scss',
})
export class AuthLayout {
  readonly title = input.required<string>();
}
