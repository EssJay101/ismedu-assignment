import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/api.models';
import { City, CityOption, CityQuery, CityRequest } from '../models/city.models';

@Injectable({ providedIn: 'root' })
export class CityService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/cities`;

  getPaged(query: CityQuery): Observable<PagedResult<City>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);

    if (query.search) {
      params = params.set('search', query.search);
    }
    if (query.countryId !== undefined) {
      params = params.set('countryId', query.countryId);
    }
    if (query.stateId !== undefined) {
      params = params.set('stateId', query.stateId);
    }
    if (query.isActive !== undefined) {
      params = params.set('isActive', query.isActive);
    }

    return this.http.get<PagedResult<City>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<City> {
    return this.http.get<City>(`${this.baseUrl}/${id}`);
  }

  getOptionsByState(stateId: number): Observable<CityOption[]> {
    return this.http.get<CityOption[]>(`${environment.apiUrl}/states/${stateId}/cities`);
  }

  create(request: CityRequest): Observable<City> {
    return this.http.post<City>(this.baseUrl, request);
  }

  update(id: number, request: CityRequest): Observable<City> {
    return this.http.put<City>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}