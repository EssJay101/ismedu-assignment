import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Subject, catchError, debounceTime, distinctUntilChanged, of, switchMap } from 'rxjs';
import { PagedResult } from '../../../core/models/api.models';
import { Country, CountryQuery } from '../../../core/models/country.models';
import { CountryService } from '../../../core/services/country.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { Pagination } from '../../../shared/pagination/pagination';
import { getErrorMessage } from '../../../shared/utils/form-errors';

type StatusFilter = 'all' | 'active' | 'inactive';

@Component({
  selector: 'app-country-list',
  imports: [RouterLink, Pagination, ConfirmDialog],
  templateUrl: './country-list.html',
  styleUrl: './country-list.css'
})
export class CountryList implements OnInit {
  private readonly countryService = inject(CountryService);
  private readonly toast = inject(ToastService);

  readonly search = signal('');
  readonly status = signal<StatusFilter>('all');
  readonly page = signal(1);
  readonly pageSize = signal(10);

  readonly result = signal<PagedResult<Country> | null>(null);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly toDelete = signal<Country | null>(null);
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
          return this.countryService.getPaged(this.buildQuery()).pipe(
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
  }

  reload(): void {
    this.reload$.next();
  }

  onSearch(event: Event): void {
    this.searchInput$.next((event.target as HTMLInputElement).value.trim());
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

  askDelete(country: Country): void {
    this.deleteError.set(null);
    this.toDelete.set(country);
  }

  cancelDelete(): void {
    this.toDelete.set(null);
  }

  confirmDelete(): void {
    const country = this.toDelete();
    if (!country) {
      return;
    }

    this.deleting.set(true);
    this.countryService.delete(country.countryId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.toDelete.set(null);
        this.toast.success(`Country '${country.countryName}' deleted.`);
        this.reload$.next();
      },
      error: (error: HttpErrorResponse) => {
        this.deleting.set(false);
        this.deleteError.set(getErrorMessage(error));
      }
    });
  }

  private buildQuery(): CountryQuery {
    const status = this.status();
    return {
      search: this.search() || undefined,
      isActive: status === 'all' ? undefined : status === 'active',
      page: this.page(),
      pageSize: this.pageSize()
    };
  }
}