import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-chart-container',
  templateUrl: './chart-container.component.html',
  styleUrls: ['./chart-container.component.scss']
})
export class ChartContainerComponent {
  @Input() title: string = '';
  @Input() subtitle: string = '';
}
