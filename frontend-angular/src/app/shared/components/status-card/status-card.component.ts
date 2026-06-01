import { Component, Input } from '@angular/core';
import { MonitorStatus } from '../../../core/models/monitor.model';

@Component({
  selector: 'app-status-card',
  templateUrl: './status-card.component.html',
  styleUrls: ['./status-card.component.scss']
})
export class StatusCardComponent {
  @Input() monitorName: string = '';
  @Input() target: string = '';
  @Input() status: MonitorStatus = MonitorStatus.Active;
  @Input() isUp: boolean = false;
  @Input() responseTime: number | null = null;
  @Input() uptimePercentage: number = 0;
  @Input() lastChecked: string | null = null;

  get statusText(): string {
    if (this.status === MonitorStatus.Paused) return 'Paused';
    if (this.status === MonitorStatus.Maintenance) return 'Maintenance';
    return this.isUp ? 'Up' : 'Down';
  }

  get statusColor(): 'green' | 'red' | 'yellow' {
    if (this.status === MonitorStatus.Paused || this.status === MonitorStatus.Maintenance) {
      return 'yellow';
    }
    return this.isUp ? 'green' : 'red';
  }
}
