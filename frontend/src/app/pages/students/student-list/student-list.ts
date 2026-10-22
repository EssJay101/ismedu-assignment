import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Subject, catchError, debounceTime, distinctUntilChanged, of, switchMap } from 'rxjs';
import { PagedResult } from '../../../core/models/api.models';
import { CityOption } from '../../../core/models/city.models';
import { CountryOption } from '../../../core/models/country.models';
import { StateOption } from '../../../core/models/state.models';
import { Student, StudentQuery } from '../../../core/models/student.models';
import { CityService } from '../../../core/services/city.service';
import { CountryService } from '../../../core/services/country.service';
import { StateService } from '../../../core/services/state.service';
import { StudentService } from '../../../core/services/student.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { Pagination } from '../../../shared/pagination/pagination';
import { getErrorMessage } from '../../../shared/utils/form-errors';

@Component({
  selector: 'app-student-list',
  imports: [RouterLink, DatePipe, Pagination, ConfirmDialog],
  templateUrl: './student-list.html',
  styleUrl: './student-list.css'
})
export class StudentList implements OnInit {
  private readonly studentService = inject(StudentService);
  private readonly countryService = inject(CountryService);
  private readonly stateService = inject(StateService);
  private readonly cityService = inject(CityService);
  private readonly toast = inject(ToastService);

  readonly search = signal('');
  readonly countryFilter = signal<number | null>(null);
  readonly stateFilter = signal<number | null>(null);
  readonly cityFilter = signal<number | null>(null);
  readonly page = signal(1);
  readonly pageSize = signal(10);

  readonly countries = signal<CountryOption[]>([]);
  readonly states = signal<StateOption[]>([]);
  readonly cities = signal<CityOption[]>([]);

  readonly result = signal<PagedResult<Student> | null>(null);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly toDelete = signal<Student | null>(null);
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  private readonly searchInput$ = new Subject<string>();
  private readonly reload$ = new Subject<void>();

  constructor() {
    this.searchInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((text) => {
        this.search.set(text);
        this.page.set(1);
        this.reload$.next();
      });

    this.reload$
      .pipe(
        switchMap(() => {
          this.loading.set(true);
          this.loadError.set(null);
          return this.studentService.getPaged(this.buildQuery()).pipe(
            catchError((error: HttpErrorResponse) => {
              this.loadError.set(getErrorMessage(error));
              return of(null);
            })
          );
        }),
        takeUntilDestroyed()
      )
      .subscribe((result) => {
        this.loading.set(false);
        if (!result) {
          return;
        }
        if (result.items.length === 0 && result.page > 1) {
          this.page.set(result.page - 1);
          this.reload$.next();
          return;
        }
        this.result.set(result);
      });
  }

  ngOnInit(): void {
    this.reload$.next();

    this.countryService.getOptions().subscribe({
      next: (countries) => this.countries.set(countries),
      error: () => this.countries.set([])
    });
  }

  reload(): void {
    this.reload$.next();
  }

  onSearch(event: Event): void {
    this.searchInput$.next((event.target as HTMLInputElement).value.trim());
  }

  onCountryFilter(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    const countryId = value ? Number(value) : null;
    this.countryFilter.set(countryId);

    this.stateFilter.set(null);
    this.cityFilter.set(null);
    this.states.set([]);
    this.cities.set([]);

    if (countryId !== null) {
      this.stateService.getOptionsByCountry(countryId).subscribe({
        next: (states) => this.states.set(states),
        error: () => this.states.set([])
      });
    }
    this.applyFilters();
  }

  onStateFilter(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    const stateId = value ? Number(value) : null;
    this.stateFilter.set(stateId);

    this.cityFilter.set(null);
    this.cities.set([]);

    if (stateId !== null) {
      this.cityService.getOptionsByState(stateId).subscribe({
        next: (cities) => this.cities.set(cities),
        error: () => this.cities.set([])
      });
    }
    this.applyFilters();
  }

  onCityFilter(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.cityFilter.set(value ? Number(value) : null);
    this.applyFilters();
  }

  goToPage(page: number): void {
    this.page.set(page);
    this.reload$.next();
  }

  changePageSize(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
    this.reload$.next();
  }

  askDelete(student: Student): void {
    this.deleteError.set(null);
    this.toDelete.set(student);
  }

  cancelDelete(): void {
    this.toDelete.set(null);
  }

  confirmDelete(): void {
    const student = this.toDelete();
    if (!student) {
      return;
    }

    this.deleting.set(true);
    this.studentService.delete(student.studentId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.toDelete.set(null);
        this.toast.success(`Student '${student.firstName} ${student.lastName}' deleted.`);
        this.reload$.next();
      },
      error: (error: HttpErrorResponse) => {
        this.deleting.set(false);
        this.deleteError.set(getErrorMessage(error));
      }
    });
  }

  private applyFilters(): void {
    this.page.set(1);
    this.reload$.next();
  }

  private buildQuery(): StudentQuery {
    return {
      search: this.search() || undefined,
      countryId: this.countryFilter() ?? undefined,
      stateId: this.stateFilter() ?? undefined,
      cityId: this.cityFilter() ?? undefined,
      page: this.page(),
      pageSize: this.pageSize()
    };
  }
}