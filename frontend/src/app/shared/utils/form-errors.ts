import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';
import { ApiError } from '../../core/models/api.models';

export function getErrorMessage(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return 'Cannot reach the server. Check that the API is running.';
  }
  const body = error.error as ApiError | null;
  return body?.message ?? 'Something went wrong. Please try again.';
}

export function applyServerErrors(form: FormGroup, error: HttpErrorResponse): string {
  const body = error.error as ApiError | null;
  const fieldErrors = body?.errors ?? {};

  for (const [field, messages] of Object.entries(fieldErrors)) {
    const control = form.get(field);
    if (control && messages.length > 0) {
      control.setErrors({ ...(control.errors ?? {}), server: messages[0] });
      control.markAsTouched();
    }
  }

  return getErrorMessage(error);
}