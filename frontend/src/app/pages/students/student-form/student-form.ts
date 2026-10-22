import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, forkJoin, map, of, switchMap } from 'rxjs';
import { CityOption } from '../../../core/models/city.models';
import { CountryOption } from '../../../core/models/country.models';
import { StateOption } from '../../../core/models/state.models';
import { Student, StudentRequest } from '../../../core/models/student.models';
import { CityService } from '../../../core/services/city.service';
import { CountryService } from '../../../core/services/country.service';
import { StateService } from '../../../core/services/state.service';
import { StudentService } from '../../../core/services/student.service';
import { ToastService } from '../../../core/services/toast.service';
import { applyServerErrors, getErrorMessage } from '../../../shared/utils/form-errors';
import { dateOfBirthValidator, toIsoDate, trimmedLength } from '../../../shared/utils/validators';

type StudentField =
  | 'firstName'
  | 'lastName'
  | 'dateOfBirth'
  | 'email'
  | 'mobile'
  | 'countryId'
  | 'stateId'
  | 'cityId'
  | 'address';

const NAME_PATTERN = /^[A-Za-z' -]+$/;
const MOBILE_PATTERN = /^[6-9][0-9]{9}$/;

@Component({
  selector: 'app-student-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './student-form.html',
  styleUrl: './student-form.css'
})
export class StudentForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly studentService = inject(StudentService);
  private readonly countryService = inject(CountryService);
  private readonly stateService = inject(StateService);
  private readonly cityService = inject(CityService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly studentId = Number(this.route.snapshot.paramMap.get('id')) || null;
  readonly isEdit = this.studentId !== null;

  readonly maxDate = (() => {
    const date = new Date();
    date.setDate(date.getDate() - 1);
    return toIsoDate(date);
  })();

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, trimmedLength(2, 50), Validators.pattern(NAME_PATTERN)]],
    lastName: ['', [Validators.required, trimmedLength(2, 50), Validators.pattern(NAME_PATTERN)]],
    dateOfBirth: ['', [Validators.required, dateOfBirthValidator()]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
    mobile: ['', [Validators.required, Validators.pattern(MOBILE_PATTERN)]],
    countryId: [null as number | null, [Validators.required]],
    stateId: [null as number | null, [Validators.required]],
    cityId: [null as number | null, [Validators.required]],
    address: ['', [Validators.required, trimmedLength(10, 250)]]
  });

  readonly countries = signal<CountryOption[]>([]);
  readonly states = signal<StateOption[]>([]);
  readonly cities = signal<CityOption[]>([]);
  readonly loadingStates = signal(false);
  readonly loadingCities = signal(false);

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

    this.form.controls.stateId.valueChanges
      .pipe(
        switchMap((stateId) => {
          this.form.controls.cityId.setValue(null);
          this.cities.set([]);
          if (stateId === null) {
            return of([] as CityOption[]);
          }
          this.loadingCities.set(true);
          return this.cityService.getOptionsByState(stateId).pipe(
            catchError((error: HttpErrorResponse) => {
              this.formError.set(getErrorMessage(error));
              return of([] as CityOption[]);
            })
          );
        }),
        takeUntilDestroyed()
      )
      .subscribe((cities) => {
        this.cities.set(cities);
        this.loadingCities.set(false);
      });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);

    const student$ = this.studentId === null ? of(null) : this.studentService.getById(this.studentId);

    forkJoin({ countries: this.countryService.getOptions(), student: student$ })
      .pipe(
        switchMap(({ countries, student }) => {
          if (student && !countries.some((c) => c.countryId === student.countryId)) {
            countries = [
              ...countries,
              { countryId: student.countryId, countryName: `${student.countryName} (inactive)` }
            ];
          }
          this.countries.set(countries);

          if (!student) {
            return of({
              student: null as Student | null,
              states: [] as StateOption[],
              cities: [] as CityOption[]
            });
          }
          return forkJoin({
            states: this.stateService.getOptionsByCountry(student.countryId),
            cities: this.cityService.getOptionsByState(student.stateId)
          }).pipe(map(({ states, cities }) => ({ student: student as Student | null, states, cities })));
        })
      )
      .subscribe({
        next: ({ student, states, cities }) => {
          if (student) {
            if (!states.some((s) => s.stateId === student.stateId)) {
              states = [...states, { stateId: student.stateId, stateName: `${student.stateName} (inactive)` }];
            }
            if (!cities.some((c) => c.cityId === student.cityId)) {
              cities = [...cities, { cityId: student.cityId, cityName: `${student.cityName} (inactive)` }];
            }
            this.states.set(states);
            this.cities.set(cities);

            this.form.patchValue(
              {
                firstName: student.firstName,
                lastName: student.lastName,
                dateOfBirth: student.dateOfBirth,
                email: student.email,
                mobile: student.mobile,
                countryId: student.countryId,
                stateId: student.stateId,
                cityId: student.cityId,
                address: student.address
              },
              { emitEvent: false }
            );
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
    const request: StudentRequest = {
      firstName: value.firstName.trim(),
      lastName: value.lastName.trim(),
      dateOfBirth: value.dateOfBirth,
      email: value.email.trim(),
      mobile: value.mobile.trim(),
      countryId: value.countryId!,
      stateId: value.stateId!,
      cityId: value.cityId!,
      address: value.address.trim()
    };

    this.saving.set(true);
    const save$ =
      this.studentId === null
        ? this.studentService.create(request)
        : this.studentService.update(this.studentId, request);

    save$.subscribe({
      next: (saved) => {
        const name = `${saved.firstName} ${saved.lastName}`;
        this.toast.success(this.isEdit ? `Student '${name}' updated.` : `Student '${name}' registered.`);
        this.router.navigate(['/students']);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(applyServerErrors(this.form, error));
      }
    });
  }

  errorFor(field: StudentField): string | null {
    const control = this.form.controls[field];
    if (!control.touched || !control.errors) {
      return null;
    }
    const errors = control.errors;

    if (errors['server']) {
      return errors['server'];
    }

    switch (field) {
      case 'firstName':
      case 'lastName': {
        const label = field === 'firstName' ? 'First name' : 'Last name';
        if (errors['required']) return `${label} is required.`;
        if (errors['trimmedLength']) return `${label} must be 2 to 50 characters.`;
        if (errors['pattern']) return `${label} can only contain letters, spaces, apostrophes and hyphens.`;
        break;
      }
      case 'dateOfBirth':
        if (errors['required']) return 'Date of birth is required.';
        if (errors['invalidDate']) return 'Enter a valid date.';
        if (errors['futureDate']) return 'Date of birth must be earlier than today.';
        if (errors['tooOld']) return 'Date of birth cannot be before 01-01-1900.';
        break;
      case 'email':
        if (errors['required']) return 'Email is required.';
        if (errors['email']) return 'Enter a valid email address.';
        if (errors['maxlength']) return 'Email cannot be longer than 254 characters.';
        break;
      case 'mobile':
        if (errors['required']) return 'Mobile number is required.';
        if (errors['pattern']) return 'Enter a 10-digit mobile number starting with 6, 7, 8 or 9.';
        break;
      case 'countryId':
        if (errors['required']) return 'Please select a country.';
        break;
      case 'stateId':
        if (errors['required']) return 'Please select a state.';
        break;
      case 'cityId':
        if (errors['required']) return 'Please select a city.';
        break;
      case 'address':
        if (errors['required']) return 'Address is required.';
        if (errors['trimmedLength']) return 'Address must be 10 to 250 characters.';
        break;
    }
    return null;
  }
}