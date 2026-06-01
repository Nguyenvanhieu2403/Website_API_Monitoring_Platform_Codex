import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResponse } from '../models/api.model';
import { MonitorLog, GetMonitorLogsParams } from '../models/monitor-log.model';

@Injectable({
  providedIn: 'root'
})
export class LogsService {
  private readonly API_URL = `${environment.apiUrl}/api/v1/logs`;

  constructor(private http: HttpClient) {}

  getMonitorLogs(params?: GetMonitorLogsParams): Observable<ApiResponse<PaginatedResponse<MonitorLog>>> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.monitorId) {
        httpParams = httpParams.set('monitorId', params.monitorId);
      }
      if (params.isUp !== undefined) {
        httpParams = httpParams.set('isUp', params.isUp.toString());
      }
      if (params.startDate) {
        httpParams = httpParams.set('startDate', params.startDate);
      }
      if (params.endDate) {
        httpParams = httpParams.set('endDate', params.endDate);
      }
      if (params.pageNumber !== undefined) {
        httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      }
      if (params.pageSize !== undefined) {
        httpParams = httpParams.set('pageSize', params.pageSize.toString());
      }
      if (params.sortBy) {
        httpParams = httpParams.set('sortBy', params.sortBy);
      }
      if (params.sortDescending !== undefined) {
        httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
      }
    }

    return this.http.get<ApiResponse<PaginatedResponse<MonitorLog>>>(this.API_URL, { params: httpParams });
  }

  getLogById(logId: string): Observable<ApiResponse<MonitorLog>> {
    return this.http.get<ApiResponse<MonitorLog>>(`${this.API_URL}/${logId}`);
  }
}
