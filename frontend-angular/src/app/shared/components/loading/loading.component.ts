import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading',
  templateUrl: './loading.component.html',
  styleUrls: ['./loading.component.scss']
})
export class LoadingComponent {
  @Input() type: 'spinner' | 'skeleton-card' | 'skeleton-table' | 'skeleton-stats' = 'spinner';
  @Input() message: string = 'Loading...';
  @Input() fullPage: boolean = false;
  @Input() count: number = 3;

  get skeletonArray(): number[] {
    return Array(this.count).fill(0).map((_, i) => i);
  }
}
