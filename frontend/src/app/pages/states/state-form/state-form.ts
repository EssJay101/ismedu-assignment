import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';

import { CountryOption } from '../../../core/models/country.models';
import { StateRequest } from '../../../core/models/state.models';
import { CountryService } from '../../../core/services/country.service';
import { StateService } from '../../../core/services/state.service';
import { ToastService } from '../../../core/services/toast.service';
import { applyServerErrors, getErrorMessage } from '../../../shared/utils/form-errors';
import { trimmedLength } from '../../../shared/utils/validators';

type StateField = 'stateName' | 'countryId';

@Component({
  selector: 'app-state-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './state-form.html',
  styleUrl: './state-form.css'
})
export class StateForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly stateService = inject(StateService);
  private readonly countryService = inject(CountryService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly stateId = Number(this.route.snapshot.paramMap.get('id')) || null;
  readonly isEdit = this.stateId !== null;

  readonly form = this.fb.nonNullable.group({
    stateName: ['', [Validators.required, trimmedLength(2, 100)]],
    countryId: [null as number | null, [Validators.required]],
    isActive: [true]
  });

  readonly countries = signal<CountryOption[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly saving = signal(false);
  readonly notFound = signal(false);
  readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    const state$ = this.stateId === null ? of(null) : this.stateService.getById(this.stateId);

    forkJoin({ countries: this.countryService.getOptions(), state: state$ }).subscribe({
      next: ({ countries, state }) => {
        if (state && !countries.some((c) => c.countryId === state.countryId)) {
          countries = [
            ...countries,
            { countryId: state.countryId, countryName: `${state.countryName} (inactive)` }
          ];
        }
        this.countries.set(countries);

        if (state) {
          this.form.patchValue({
            stateName: state.stateName,
            countryId: state.countryId,
            isActive: state.isActive
          });
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
    const request: StateRequest = {
      stateName: value.stateName.trim(),
      countryId: value.countryId!,
      isActive: value.isActive
    };

    this.saving.set(true);
    const save$ =
      this.stateId === null
        ? this.stateService.create(request)
        : this.stateService.update(this.stateId, request);

    save$.subscribe({
      next: (saved) => {
        this.toast.success(`State '${saved.stateName}' ${this.isEdit ? 'updated' : 'added'}.`);
        this.router.navigate(['/states']);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      }
    });
  }

  errorFor(field: StateField): string | null {
    const control = this.form.controls[field];
    if (!control.touched || !control.errors) {
      return null;
    }
    const errors = control.errors;

    if (errors['server']) {
      return errors['server'];
    }
    if (field === 'stateName') {
      if (errors['required']) return 'State name is required.';
      if (errors['trimmedLength']) return 'State name must be 2 to 100 characters.';
    }
    if (field === 'countryId' && errors['required']) {
      return 'Please select a country.';
    }
    return null;
  }
}