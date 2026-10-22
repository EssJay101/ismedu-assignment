import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function trimmedLength(min: number, max: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = String(control.value ?? '').trim();
    if (value.length === 0) {
      return null;
    }
    return value.length < min || value.length > max ? { trimmedLength: { min, max } } : null;
  };
}

export function toIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function dateOfBirthValidator(oldest = '1900-01-01'): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string;
    if (!value) {
      return null;
    }
    if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) {
      return { invalidDate: true };
    }
    if (value >= toIsoDate(new Date())) {
      return { futureDate: true };
    }
    if (value < oldest) {
      return { tooOld: true };
    }
    return null;
  };
}