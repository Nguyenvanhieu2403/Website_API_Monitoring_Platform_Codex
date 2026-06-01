import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly ACCESS_TOKEN_EXPIRY_KEY = 'access_token_expiry';
  private readonly REFRESH_TOKEN_EXPIRY_KEY = 'refresh_token_expiry';

  // Access token stored in memory for security
  private accessToken: string | null = null;

  constructor() {}

  setAccessToken(token: string, expiration: string): void {
    this.accessToken = token;
    localStorage.setItem(this.ACCESS_TOKEN_EXPIRY_KEY, expiration);
  }

  getAccessToken(): string | null {
    return this.accessToken;
  }

  setRefreshToken(token: string, expiration: string): void {
    localStorage.setItem(this.REFRESH_TOKEN_KEY, token);
    localStorage.setItem(this.REFRESH_TOKEN_EXPIRY_KEY, expiration);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  clearTokens(): void {
    this.accessToken = null;
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.ACCESS_TOKEN_EXPIRY_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_EXPIRY_KEY);
  }

  isAccessTokenExpired(): boolean {
    const expiry = localStorage.getItem(this.ACCESS_TOKEN_EXPIRY_KEY);
    if (!expiry) return true;
    return new Date(expiry) <= new Date();
  }

  isRefreshTokenExpired(): boolean {
    const expiry = localStorage.getItem(this.REFRESH_TOKEN_EXPIRY_KEY);
    if (!expiry) return true;
    return new Date(expiry) <= new Date();
  }
}
