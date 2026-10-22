import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/api.models';
import { Country, CountryOption, CountryQuery, CountryRequest } from '../models/country.models';

@Injectable({ providedIn: 'root' })
export class CountryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/countries`;

  getPaged(query: CountryQuery): Observable<PagedResult<Country>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);

    if (query.search) {
      params = params.set('search', query.search);
    }
    if (query.isActive !== undefined) {
      params = params.set('isActive', query.isActive);
    }

    return this.http.get<PagedResult<Country>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<Country> {
    return this.http.get<Country>(`${this.baseUrl}/${id}`);
  }

  getOptions(): Observable<CountryOption[]> {
    return this.http.get<CountryOption[]>(`${this.baseUrl}/options`);
  }

  create(request: CountryRequest): Observable<Country> {
    return this.http.post<Country>(this.baseUrl, request);
  }

  update(id: number, request: CountryRequest): Observable<Country> {
    return this.http.put<Country>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}