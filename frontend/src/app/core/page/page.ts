import { ChangeDetectionStrategy, Component, input, ViewEncapsulation } from '@angular/core';

export type PageSize = 'default' | 'wide' | 'narrow';

@Component({
  selector: 'app-page',
  templateUrl: './page.html',
  styleUrl: './page.scss',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Page {
  readonly title = input.required<string>();
  readonly lead = input<string>();
  readonly size = input<PageSize>('default');
}
