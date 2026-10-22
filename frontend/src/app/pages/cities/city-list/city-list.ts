import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Subject, catchError, debounceTime, distinctUntilChanged, of, switchMap } from 'rxjs';
import { PagedResult } from '../../../core/models/api.models';
import { City, CityQuery } from '../../../core/models/city.models';
import { CountryOption } from '../../../core/models/country.models';
import { StateOption } from '../../../core/models/state.models';
import { CityService } from '../../../core/services/city.service';
import { CountryService } from '../../../core/services/country.service';
import { StateService } from '../../../core/services/state.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { Pagination } from '../../../shared/pagination/pagination';
import { getErrorMessage } from '../../../shared/utils/form-errors';

type StatusFilter = 'all' | 'active' | 'inactive';

@Component({
  selector: 'app-city-list',
  imports: [RouterLink, Pagination, ConfirmDialog],
  templateUrl: './city-list.html',
  styleUrl: './city-list.css'
})
export class CityList implements OnInit {
  private readonly cityService = inject(CityService);
  private readonly countryService = inject(CountryService);
  private readonly stateService = inject(StateService);
  private readonly toast = inject(ToastService);

  readonly search = signal('');
  readonly countryFilter = signal<number | null>(null);
  readonly stateFilter = signal<number | null>(null);
  readonly status = signal<StatusFilter>('all');
  readonly page = signal(1);
  readonly pageSize = signal(10);

  readonly countries = signal<CountryOption[]>([]);
  readonly states = signal<StateOption[]>([]);

  readonly result = signal<PagedResult<City> | null>(null);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly toDelete = signal<City | null>(null);
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
          return this.cityService.getPaged(this.buildQuery()).pipe(
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
    this.states.set([]);
    if (countryId !== null) {
      this.stateService.getOptionsByCountry(countryId).subscribe({
        next: (states) => this.states.set(states),
        error: () => this.states.set([])
      });
    }

    this.page.set(1);
    this.reload$.next();
  }

  onStateFilter(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.stateFilter.set(value ? Number(value) : null);
    this.page.set(1);
    this.reload$.next();
  }

  onStatus(event: Event): void {
    this.status.set((event.target as HTMLSelectElement).value as StatusFilter);
    this.page.set(1);
    this.reload$.next();
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

  askDelete(city: City): void {
    this.deleteError.set(null);
    this.toDelete.set(city);
  }

  cancelDelete(): void {
    this.toDelete.set(null);
  }

  confirmDelete(): void {
    const city = this.toDelete();
    if (!city) {
      return;
    }

    this.deleting.set(true);
    this.cityService.delete(city.cityId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.toDelete.set(null);
        this.toast.success(`City '${city.cityName}' deleted.`);
        this.reload$.next();
      },
      error: (error: HttpErrorResponse) => {
        this.deleting.set(false);
        this.deleteError.set(getErrorMessage(error));
      }
    });
  }

  private buildQuery(): CityQuery {
    const status = this.status();
    return {
      search: this.search() || undefined,
      countryId: this.countryFilter() ?? undefined,
      stateId: this.stateFilter() ?? undefined,
      isActive: status === 'all' ? undefined : status === 'active',
      page: this.page(),
      pageSize: this.pageSize()
    };
  }
}