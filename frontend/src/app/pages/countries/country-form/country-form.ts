import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CountryRequest } from '../../../core/models/country.models';
import { CountryService } from '../../../core/services/country.service';
import { ToastService } from '../../../core/services/toast.service';
import { applyServerErrors, getErrorMessage } from '../../../shared/utils/form-errors';
import { trimmedLength } from '../../../shared/utils/validators';

type CountryField = 'countryName' | 'countryCode';

@Component({
  selector: 'app-country-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './country-form.html',
  styleUrl: './country-form.css'
})
export class CountryForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly countryService = inject(CountryService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly countryId = Number(this.route.snapshot.paramMap.get('id')) || null;
  readonly isEdit = this.countryId !== null;

  readonly form = this.fb.nonNullable.group({
    countryName: ['', [Validators.required, trimmedLength(2, 100)]],
    countryCode: ['', [Validators.pattern(/^[A-Za-z]{2,3}$/)]],
    isActive: [true]
  });

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly notFound = signal(false);
  readonly formError = signal<string | null>(null);

  ngOnInit(): void {
    if (this.countryId === null) {
      return;
    }

    this.loading.set(true);
    this.countryService.getById(this.countryId).subscribe({
      next: (country) => {
        this.form.patchValue({
          countryName: country.countryName,
          countryCode: country.countryCode ?? '',
          isActive: country.isActive
        });
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        if (error.status === 404) {
          this.notFound.set(true);
        } else {
          this.formError.set(getErrorMessage(error));
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
    const code = value.countryCode.trim();
    const request: CountryRequest = {
      countryName: value.countryName.trim(),
      countryCode: code ? code.toUpperCase() : null,
      isActive: value.isActive
    };

    this.saving.set(true);
    const save$ =
      this.countryId === null
        ? this.countryService.create(request)
        : this.countryService.update(this.countryId, request);

    save$.subscribe({
      next: (saved) => {
        this.toast.success(`Country '${saved.countryName}' ${this.isEdit ? 'updated' : 'added'}.`);
        this.router.navigate(['/countries']);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      }
    });
  }

  errorFor(field: CountryField): string | null {
    const control = this.form.controls[field];
    if (!control.touched || !control.errors) {
      return null;
    }
    const errors = control.errors;

    if (errors['server']) {
      return errors['server'];
    }
    if (field === 'countryName') {
      if (errors['required']) return 'Country name is required.';
      if (errors['trimmedLength']) return 'Country name must be 2 to 100 characters.';
    }
    if (field === 'countryCode' && errors['pattern']) {
      return 'Country code must be 2 or 3 letters.';
    }
    return null;
  }
}