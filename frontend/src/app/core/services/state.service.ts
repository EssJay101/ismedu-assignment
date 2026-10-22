import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/api.models';
import { State, StateOption, StateQuery, StateRequest } from '../models/state.models';

@Injectable({ providedIn: 'root' })
export class StateService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/states`;

  getPaged(query: StateQuery): Observable<PagedResult<State>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);

    if (query.search) {
      params = params.set('search', query.search);
    }
    if (query.countryId !== undefined) {
      params = params.set('countryId', query.countryId);
    }
    if (query.isActive !== undefined) {
      params = params.set('isActive', query.isActive);
    }

    return this.http.get<PagedResult<State>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<State> {
    return this.http.get<State>(`${this.baseUrl}/${id}`);
  }

  getOptionsByCountry(countryId: number): Observable<StateOption[]> {
    return this.http.get<StateOption[]>(`${environment.apiUrl}/countries/${countryId}/states`);
  }

  create(request: StateRequest): Observable<State> {
    return this.http.post<State>(this.baseUrl, request);
  }

  update(id: number, request: StateRequest): Observable<State> {
    return this.http.put<State>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}