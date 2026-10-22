import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Subject, catchError, debounceTime, distinctUntilChanged, of, switchMap } from 'rxjs';
import { PagedResult } from '../../../core/models/api.models';
import { CountryOption } from '../../../core/models/country.models';
import { State, StateQuery } from '../../../core/models/state.models';
import { CountryService } from '../../../core/services/country.service';
import { StateService } from '../../../core/services/state.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { Pagination } from '../../../shared/pagination/pagination';
import { getErrorMessage } from '../../../shared/utils/form-errors';

type StatusFilter = 'all' | 'active' | 'inactive';

@Component({
  selector: 'app-state-list',
  imports: [RouterLink, Pagination, ConfirmDialog],
  templateUrl: './state-list.html',
  styleUrl: './state-list.css'
})
export class StateList implements OnInit {
  private readonly stateService = inject(StateService);
  private readonly countryService = inject(CountryService);
  private readonly toast = inject(ToastService);

  readonly search = signal('');
  readonly countryFilter = signal<number | null>(null);
  readonly status = signal<StatusFilter>('all');
  readonly page = signal(1);
  readonly pageSize = signal(10);

  readonly countries = signal<CountryOption[]>([]);

  readonly result = signal<PagedResult<State> | null>(null);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly toDelete = signal<State | null>(null);
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
          return this.stateService.getPaged(this.buildQuery()).pipe(
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
    this.countryFilter.set(value ? Number(value) : null);
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

  askDelete(state: State): void {
    this.deleteError.set(null);
    this.toDelete.set(state);
  }

  cancelDelete(): void {
    this.toDelete.set(null);
  }

  confirmDelete(): void {
    const state = this.toDelete();
    if (!state) {
      return;
    }

    this.deleting.set(true);
    this.stateService.delete(state.stateId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.toDelete.set(null);
        this.toast.success(`State '${state.stateName}' deleted.`);
        this.reload$.next();
      },
      error: (error: HttpErrorResponse) => {
        this.deleting.set(false);
        this.deleteError.set(getErrorMessage(error));
      }
    });
  }

  private buildQuery(): StateQuery {
    const status = this.status();
    return {
      search: this.search() || undefined,
      countryId: this.countryFilter() ?? undefined,
      isActive: status === 'all' ? undefined : status === 'active',
      page: this.page(),
      pageSize: this.pageSize()
    };
  }
}