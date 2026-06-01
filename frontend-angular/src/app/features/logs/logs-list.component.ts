import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LogsService } from '../../core/services/logs.service';
import { MonitorLog } from '../../core/models/monitor-log.model';

@Component({
  selector: 'app-logs-list',
  templateUrl: './logs-list.component.html',
  styleUrls: ['./logs-list.component.scss']
})
export class LogsListComponent implements OnInit {
  logs: MonitorLog[] = [];
  isLoading = true;
  errorMessage = '';

  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  selectedMonitorId = '';
  selectedIsUp: boolean | undefined;
  startDate = '';
  endDate = '';

  constructor(
    private logsService: LogsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.logsService.getMonitorLogs({
      monitorId: this.selectedMonitorId || undefined,
      isUp: this.selectedIsUp,
      startDate: this.startDate || undefined,
      endDate: this.endDate || undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: 'CheckedAt',
      sortDescending: true
    }).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.logs = response.data.items as MonitorLog[];
          this.totalCount = response.data.totalCount;
          this.totalPages = response.data.totalPages;
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load logs';
        this.isLoading = false;
      }
    });
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadLogs();
  }

  clearFilters(): void {
    this.selectedMonitorId = '';
    this.selectedIsUp = undefined;
    this.startDate = '';
    this.endDate = '';
    this.currentPage = 1;
    this.loadLogs();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadLogs();
  }

  viewLogDetails(logId: string): void {
    this.router.navigate(['/logs', logId]);
  }

  getStatusColor(isUp: boolean): 'green' | 'red' {
    return isUp ? 'green' : 'red';
  }

  getStatusText(isUp: boolean): string {
    return isUp ? 'Success' : 'Failed';
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  getEndRecord(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalCount);
  }
}
