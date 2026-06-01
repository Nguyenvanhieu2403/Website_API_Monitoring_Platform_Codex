import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-error-state',
  templateUrl: './error-state.component.html',
  styleUrls: ['./error-state.component.scss']
})
export class ErrorStateComponent {
  @Input() icon: string = '⚠️';
  @Input() title: string = 'Something went wrong';
  @Input() message: string = 'An error occurred while loading the data. Please try again.';
  @Input() details: string = '';
  @Input() showRetry: boolean = true;
  @Input() showDetails: boolean = false;
  @Input() compact: boolean = false;
  @Input() inline: boolean = false;
  @Output() retry = new EventEmitter<void>();

  detailsExpanded: boolean = false;

  onRetryClick(): void {
    this.retry.emit();
  }

  toggleDetails(): void {
    this.detailsExpanded = !this.detailsExpanded;
  }
}
