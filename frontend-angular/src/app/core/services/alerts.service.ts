import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.model';
import { AlertRule, CreateAlertRuleRequest, UpdateAlertRuleRequest } from '../models/alert-rule.model';

@Injectable({
  providedIn: 'root'
})
export class AlertsService {
  private readonly API_URL = `${environment.apiUrl}/api/organizations`;

  constructor(private http: HttpClient) {}

  getAlertRules(organizationId: string, monitorId: string): Observable<ApiResponse<AlertRule[]>> {
    return this.http.get<ApiResponse<AlertRule[]>>(
      `${this.API_URL}/${organizationId}/monitors/${monitorId}/alert-rules`
    );
  }

  getAlertRuleById(organizationId: string, ruleId: string): Observable<ApiResponse<AlertRule>> {
    return this.http.get<ApiResponse<AlertRule>>(
      `${this.API_URL}/${organizationId}/monitors/any/alert-rules/${ruleId}`
    );
  }

  createAlertRule(organizationId: string, monitorId: string, request: CreateAlertRuleRequest): Observable<ApiResponse<AlertRule>> {
    return this.http.post<ApiResponse<AlertRule>>(
      `${this.API_URL}/${organizationId}/monitors/${monitorId}/alert-rules`,
      request
    );
  }

  updateAlertRule(organizationId: string, monitorId: string, ruleId: string, request: UpdateAlertRuleRequest): Observable<ApiResponse<AlertRule>> {
    return this.http.put<ApiResponse<AlertRule>>(
      `${this.API_URL}/${organizationId}/monitors/${monitorId}/alert-rules/${ruleId}`,
      request
    );
  }

  deleteAlertRule(organizationId: string, ruleId: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(
      `${this.API_URL}/${organizationId}/monitors/any/alert-rules/${ruleId}`
    );
  }
}
