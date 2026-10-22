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