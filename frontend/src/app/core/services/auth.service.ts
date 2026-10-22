import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from '../models/auth.models';

const STORAGE_KEY = 'ismedu_auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly currentUser = signal<LoginResponse | null>(this.readFromStorage());

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/login`, request)
      .pipe(
        tap((response) => {
          localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
          this.currentUser.set(response);
        })
      );
  }

  logout(reason?: string): void {
    localStorage.removeItem(STORAGE_KEY);
    this.currentUser.set(null);
    this.router.navigate(['/login'], reason ? { queryParams: { reason } } : {});
  }

  getToken(): string | null {
    return this.isLoggedIn() ? this.currentUser()!.token : null;
  }

  isLoggedIn(): boolean {
    const user = this.currentUser();
    if (!user) {
      return false;
    }
    return new Date(user.expiresAt).getTime() > Date.now();
  }

  private readFromStorage(): LoginResponse | null {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      return saved ? (JSON.parse(saved) as LoginResponse) : null;
    } catch {
      return null;
    }
  }
}