import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, forkJoin, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/api.models';

export interface DashboardCounts {
  countries: number | null;
  states: number | null;
  cities: number | null;
  students: number | null;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  getCounts(): Observable<DashboardCounts> {
    return forkJoin({
      countries: this.count('countries'),
      states: this.count('states'),
      cities: this.count('cities'),
      students: this.count('students')
    });
  }

  private count(resource: string): Observable<number | null> {
    const params = new HttpParams().set('pageSize', 1);

    return this.http
      .get<PagedResult<unknown>>(`${environment.apiUrl}/${resource}`, { params })
      .pipe(
        map((result) => result.totalCount),
        catchError(() => of(null))
      );
  }
}