import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedResponse } from '../models/api.model';
import { User, CreateUserRequest, UpdateUserRequest, GetUsersParams, UpdateProfileRequest, ChangePasswordRequest } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private readonly API_URL = `${environment.apiUrl}/api/v1/users`;

  constructor(private http: HttpClient) {}

  getUsers(params?: GetUsersParams): Observable<ApiResponse<PaginatedResponse<User>>> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.organizationId) {
        httpParams = httpParams.set('organizationId', params.organizationId);
      }
      if (params.role !== undefined) {
        httpParams = httpParams.set('role', params.role.toString());
      }
      if (params.status !== undefined) {
        httpParams = httpParams.set('status', params.status.toString());
      }
      if (params.searchTerm) {
        httpParams = httpParams.set('searchTerm', params.searchTerm);
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

    return this.http.get<ApiResponse<PaginatedResponse<User>>>(this.API_URL, { params: httpParams });
  }

  getUserById(userId: string): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.API_URL}/${userId}`);
  }

  createUser(request: CreateUserRequest): Observable<ApiResponse<User>> {
    return this.http.post<ApiResponse<User>>(this.API_URL, request);
  }

  updateUser(userId: string, request: UpdateUserRequest): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.API_URL}/${userId}`, request);
  }

  deleteUser(userId: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.API_URL}/${userId}`);
  }

  getCurrentUser(): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${environment.apiUrl}/api/v1/auth/me`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${environment.apiUrl}/api/v1/auth/profile`, request);
  }

  changePassword(request: ChangePasswordRequest): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${environment.apiUrl}/api/v1/auth/change-password`, request);
  }
}
