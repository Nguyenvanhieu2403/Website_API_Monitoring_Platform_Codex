import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/auth.model';
import { DashboardSummary, RecentMonitor } from '../models/dashboard.model';
import { PagedResponse } from '../models/monitor.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly API_URL = `${environment.apiUrl}/api/v1`;

  constructor(private http: HttpClient) {}

  getDashboardSummary(timeRange: string = '24h'): Observable<ApiResponse<DashboardSummary>> {
    return this.http.get<ApiResponse<DashboardSummary>>(
      `${this.API_URL}/dashboard/summary?timeRange=${timeRange}`
    );
  }

  getRecentMonitors(pageNumber: number = 1, pageSize: number = 5): Observable<ApiResponse<PagedResponse<RecentMonitor>>> {
    return this.http.get<ApiResponse<PagedResponse<RecentMonitor>>>(
      `${this.API_URL}/monitors?pageNumber=${pageNumber}&pageSize=${pageSize}&sortBy=LastCheckedAt&sortDescending=true`
    );
  }
}
