import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/auth.model';
import { Monitor, CreateMonitorRequest, UpdateMonitorRequest, PagedResponse, MonitorType, MonitorStatus } from '../models/monitor.model';

export interface GetMonitorsListParams {
  search?: string;
  type?: MonitorType;
  status?: MonitorStatus;
  categoryId?: string;
  tagId?: string;
  isUp?: boolean;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class MonitorsService {
  private readonly API_URL = `${environment.apiUrl}/api/v1/monitors`;

  constructor(private http: HttpClient) {}

  getMonitors(params?: GetMonitorsListParams): Observable<ApiResponse<PagedResponse<Monitor>>> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.search) httpParams = httpParams.set('search', params.search);
      if (params.type !== undefined) httpParams = httpParams.set('type', params.type.toString());
      if (params.status !== undefined) httpParams = httpParams.set('status', params.status.toString());
      if (params.categoryId) httpParams = httpParams.set('categoryId', params.categoryId);
      if (params.tagId) httpParams = httpParams.set('tagId', params.tagId);
      if (params.isUp !== undefined) httpParams = httpParams.set('isUp', params.isUp.toString());
      if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
      if (params.sortDescending !== undefined) httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
    }

    return this.http.get<ApiResponse<PagedResponse<Monitor>>>(this.API_URL, { params: httpParams });
  }

  getMonitorById(id: string): Observable<ApiResponse<Monitor>> {
    return this.http.get<ApiResponse<Monitor>>(`${this.API_URL}/${id}`);
  }

  createMonitor(request: CreateMonitorRequest): Observable<ApiResponse<Monitor>> {
    return this.http.post<ApiResponse<Monitor>>(this.API_URL, request);
  }

  updateMonitor(id: string, request: UpdateMonitorRequest): Observable<ApiResponse<Monitor>> {
    return this.http.put<ApiResponse<Monitor>>(`${this.API_URL}/${id}`, request);
  }

  deleteMonitor(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.API_URL}/${id}`);
  }
}
