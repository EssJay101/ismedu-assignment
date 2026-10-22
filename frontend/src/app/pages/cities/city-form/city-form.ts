import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, forkJoin, map, of, switchMap } from 'rxjs';

import { City, CityRequest } from '../../../core/models/city.models';
import { CountryOption } from '../../../core/models/country.models';
import { StateOption } from '../../../core/models/state.models';
import { CityService } from '../../../core/services/city.service';
import { CountryService } from '../../../core/services/country.service';
import { StateService } from '../../../core/services/state.service';
import { ToastService } from '../../../core/services/toast.service';
import { applyServerErrors, getErrorMessage } from '../../../shared/utils/form-errors';
import { trimmedLength } from '../../../shared/utils/validators';

type CityField = 'cityName' | 'countryId' | 'stateId';

@Component({
  selector: 'app-city-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './city-form.html',
  styleUrl: './city-form.css'
})
export class CityForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly cityService = inject(CityService);
  private readonly countryService = inject(CountryService);
  private readonly stateService = inject(StateService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly cityId = Number(this.route.snapshot.paramMap.get('id')) || null;
  readonly isEdit = this.cityId !== null;

  readonly form = this.fb.nonNullable.group({
    cityName: ['', [Validators.required, trimmedLength(2, 100)]],
    countryId: [null as number | null, [Validators.required]],
    stateId: [null as number | null, [Validators.required]],
    isActive: [true]
  });

  readonly countries = signal<CountryOption[]>([]);
  readonly states = signal<StateOption[]>([]);
  readonly loadingStates = signal(false);

  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly saving = signal(false);
  readonly notFound = signal(false);
  readonly formError = signal<string | null>(null);

  constructor() {
    this.form.controls.countryId.valueChanges
      .pipe(
        switchMap((countryId) => {
          this.form.controls.stateId.setValue(null);
          this.states.set([]);
          if (countryId === null) {
            return of([] as StateOption[]);
          }
          this.loadingStates.set(true);
          return this.stateService.getOptionsByCountry(countryId).pipe(
            catchError((error: HttpErrorResponse) => {
              this.formError.set(getErrorMessage(error));
              return of([] as StateOption[]);
            })
          );
        }),
        takeUntilDestroyed()
      )
      .subscribe((states) => {
        this.states.set(states);
        this.loadingStates.set(false);
      });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    const city$ = this.cityId === null ? of(null) : this.cityService.getById(this.cityId);

    forkJoin({ countries: this.countryService.getOptions(), city: city$ })
      .pipe(
        switchMap(({ countries, city }) => {
          if (city && !countries.some((c) => c.countryId === city.countryId)) {
            countries = [
              ...countries,
              { countryId: city.countryId, countryName: `${city.countryName} (inactive)` }
            ];
          }
          this.countries.set(countries);

          if (!city) {
            return of({ city: null as City | null, states: [] as StateOption[] });
          }
          // Edit mode: also load the states of the city's country
          return this.stateService
            .getOptionsByCountry(city.countryId)
            .pipe(map((states) => ({ city: city as City | null, states })));
        })
      )
      .subscribe({
        next: ({ city, states }) => {
          if (city) {
            if (!states.some((s) => s.stateId === city.stateId)) {
              states = [...states, { stateId: city.stateId, stateName: `${city.stateName} (inactive)` }];
            }
            this.states.set(states);

            // emitEvent: false → don't trigger the "country changed" logic above
            this.form.patchValue(
              { cityName: city.cityName, countryId: city.countryId, isActive: city.isActive },
              { emitEvent: false }
            );
            this.form.controls.stateId.setValue(city.stateId);
          }
          this.loading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.loading.set(false);
          if (error.status === 404) {
            this.notFound.set(true);
          } else {
            this.loadError.set(getErrorMessage(error));
          }
        }
      });
  }

  submit(): void {
    this.formError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: CityRequest = {
      cityName: value.cityName.trim(),
      stateId: value.stateId!,
      isActive: value.isActive
    };

    this.saving.set(true);
    const save$ =
      this.cityId === null
        ? this.cityService.create(request)
        : this.cityService.update(this.cityId, request);

    save$.subscribe({
      next: (saved) => {
        this.toast.success(`City '${saved.cityName}' ${this.isEdit ? 'updated' : 'added'}.`);
        this.router.navigate(['/cities']);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      }
    });
  }

  errorFor(field: CityField): string | null {
    const control = this.form.controls[field];
    if (!control.touched || !control.errors) {
      return null;
    }
    const errors = control.errors;

    if (errors['server']) {
      return errors['server'];
    }
    if (field === 'cityName') {
      if (errors['required']) return 'City name is required.';
      if (errors['trimmedLength']) return 'City name must be 2 to 100 characters.';
    }
    if (field === 'countryId' && errors['required']) {
      return 'Please select a country.';
    }
    if (field === 'stateId' && errors['required']) {
      return 'Please select a state.';
    }
    return null;
  }
}