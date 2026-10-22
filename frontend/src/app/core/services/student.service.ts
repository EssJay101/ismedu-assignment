import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/api.models';
import { Student, StudentQuery, StudentRequest } from '../models/student.models';

@Injectable({ providedIn: 'root' })
export class StudentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/students`;

  getPaged(query: StudentQuery): Observable<PagedResult<Student>> {
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
    if (query.cityId !== undefined) {
      params = params.set('cityId', query.cityId);
    }

    return this.http.get<PagedResult<Student>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<Student> {
    return this.http.get<Student>(`${this.baseUrl}/${id}`);
  }

  create(request: StudentRequest): Observable<Student> {
    return this.http.post<Student>(this.baseUrl, request);
  }

  update(id: number, request: StudentRequest): Observable<Student> {
    return this.http.put<Student>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}