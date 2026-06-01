import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MonitorsService } from '../../core/services/monitors.service';
import { Monitor, MonitorType, MonitorStatus } from '../../core/models/monitor.model';

@Component({
  selector: 'app-monitor-detail',
  templateUrl: './monitor-detail.component.html',
  styleUrls: ['./monitor-detail.component.scss']
})
export class MonitorDetailComponent implements OnInit {
  monitor: Monitor | null = null;
  isLoading = true;
  errorMessage = '';
  monitorId: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private monitorsService: MonitorsService
  ) {}

  ngOnInit(): void {
    this.monitorId = this.route.snapshot.paramMap.get('id') || '';
    if (this.monitorId) {
      this.loadMonitor();
    } else {
      this.errorMessage = 'Invalid monitor ID';
      this.isLoading = false;
    }
  }

  loadMonitor(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.monitorsService.getMonitorById(this.monitorId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.monitor = response.data;
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load monitor details';
        this.isLoading = false;
      }
    });
  }

  editMonitor(): void {
    this.router.navigate(['/monitors', this.monitorId, 'edit']);
  }

  deleteMonitor(): void {
    if (!this.monitor) return;

    if (!confirm(`Are you sure you want to delete "${this.monitor.name}"?`)) {
      return;
    }

    this.monitorsService.deleteMonitor(this.monitorId).subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/monitors']);
        }
      },
      error: (error) => {
        alert(error.error?.message || 'Failed to delete monitor');
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/monitors']);
  }

  getMonitorTypeLabel(type: MonitorType): string {
    switch (type) {
      case MonitorType.Http: return 'HTTP';
      case MonitorType.Https: return 'HTTPS';
      case MonitorType.ApiEndpoint: return 'API Endpoint';
      case MonitorType.TcpPort: return 'TCP Port';
      case MonitorType.Ping: return 'Ping';
      default: return 'Unknown';
    }
  }

  getStatusColor(): 'green' | 'red' | 'yellow' {
    if (!this.monitor) return 'yellow';
    if (this.monitor.status === MonitorStatus.Paused || this.monitor.status === MonitorStatus.Maintenance) {
      return 'yellow';
    }
    return this.monitor.isUp ? 'green' : 'red';
  }

  getStatusText(): string {
    if (!this.monitor) return 'Unknown';
    if (this.monitor.status === MonitorStatus.Paused) return 'Paused';
    if (this.monitor.status === MonitorStatus.Maintenance) return 'Maintenance';
    if (this.monitor.status === MonitorStatus.Down) return 'Down';
    return this.monitor.isUp ? 'Up' : 'Down';
  }

  getHealthStatus(): { label: string; color: string } {
    if (!this.monitor) return { label: 'Unknown', color: '#6b7280' };

    const uptime = this.monitor.uptimePercentage;
    if (uptime >= 99.9) return { label: 'Excellent', color: '#10b981' };
    if (uptime >= 99) return { label: 'Good', color: '#3b82f6' };
    if (uptime >= 95) return { label: 'Fair', color: '#f59e0b' };
    return { label: 'Poor', color: '#ef4444' };
  }
}
