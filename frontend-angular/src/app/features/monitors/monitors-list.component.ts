import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MonitorsService } from '../../core/services/monitors.service';
import { Monitor, MonitorType, MonitorStatus } from '../../core/models/monitor.model';

@Component({
  selector: 'app-monitors-list',
  templateUrl: './monitors-list.component.html',
  styleUrls: ['./monitors-list.component.scss']
})
export class MonitorsListComponent implements OnInit {
  monitors: Monitor[] = [];
  isLoading = true;
  errorMessage = '';

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // Filters
  searchTerm = '';
  selectedType: MonitorType | undefined;
  selectedStatus: MonitorStatus | undefined;
  selectedIsUp: boolean | undefined;

  // Enums for template
  MonitorType = MonitorType;
  MonitorStatus = MonitorStatus;

  constructor(
    private monitorsService: MonitorsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadMonitors();
  }

  loadMonitors(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.monitorsService.getMonitors({
      search: this.searchTerm || undefined,
      type: this.selectedType,
      status: this.selectedStatus,
      isUp: this.selectedIsUp,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: 'CreatedAt',
      sortDescending: true
    }).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.monitors = response.data.items as Monitor[];
          this.totalCount = response.data.totalCount;
          this.totalPages = response.data.totalPages;
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load monitors';
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadMonitors();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadMonitors();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedType = undefined;
    this.selectedStatus = undefined;
    this.selectedIsUp = undefined;
    this.currentPage = 1;
    this.loadMonitors();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadMonitors();
  }

  viewDetails(monitorId: string): void {
    this.router.navigate(['/monitors', monitorId]);
  }

  editMonitor(monitorId: string): void {
    this.router.navigate(['/monitors', monitorId, 'edit']);
  }

  deleteMonitor(monitor: Monitor): void {
    if (!confirm(`Are you sure you want to delete "${monitor.name}"?`)) {
      return;
    }

    this.monitorsService.deleteMonitor(monitor.monitorId).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadMonitors();
        }
      },
      error: (error) => {
        alert(error.error?.message || 'Failed to delete monitor');
      }
    });
  }

  createMonitor(): void {
    this.router.navigate(['/monitors/new']);
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

  getStatusColor(monitor: Monitor): 'green' | 'red' | 'yellow' {
    if (monitor.status === MonitorStatus.Paused || monitor.status === MonitorStatus.Maintenance) {
      return 'yellow';
    }
    return monitor.isUp ? 'green' : 'red';
  }

  getStatusText(monitor: Monitor): string {
    if (monitor.status === MonitorStatus.Paused) return 'Paused';
    if (monitor.status === MonitorStatus.Maintenance) return 'Maintenance';
    if (monitor.status === MonitorStatus.Down) return 'Down';
    return monitor.isUp ? 'Up' : 'Down';
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  getEndRecord(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalCount);
  }
}
