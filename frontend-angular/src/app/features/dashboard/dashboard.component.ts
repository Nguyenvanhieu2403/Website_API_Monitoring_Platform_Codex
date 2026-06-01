import { Component, OnInit } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardSummary, RecentMonitor } from '../../core/models/dashboard.model';
import { MonitorStatus } from '../../core/models/monitor.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  summary: DashboardSummary | null = null;
  recentMonitors: RecentMonitor[] = [];
  isLoading = true;
  errorMessage = '';
  selectedTimeRange: string = '24h';

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.dashboardService.getDashboardSummary(this.selectedTimeRange).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.summary = response.data;
        }
        this.loadRecentMonitors();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load dashboard data';
        this.isLoading = false;
      }
    });
  }

  loadRecentMonitors(): void {
    this.dashboardService.getRecentMonitors(1, 5).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.recentMonitors = response.data.items;
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load recent monitors';
        this.isLoading = false;
      }
    });
  }

  onTimeRangeChange(timeRange: string): void {
    this.selectedTimeRange = timeRange;
    this.loadDashboardData();
  }

  get monitorsUp(): number {
    return this.recentMonitors.filter(m => m.isUp).length;
  }

  get monitorsDown(): number {
    return this.recentMonitors.filter(m => !m.isUp && m.status !== MonitorStatus.Paused).length;
  }

  get monitorsPaused(): number {
    return this.recentMonitors.filter(m => m.status === MonitorStatus.Paused).length;
  }
}
