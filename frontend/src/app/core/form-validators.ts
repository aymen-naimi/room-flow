import { AbstractControl, ValidationErrors } from '@angular/forms';

export class FormValidators {
  public static requiredText(control: AbstractControl): ValidationErrors | null {
    if (typeof control.value !== 'string' || control.value.trim() === '') {
      return { required: true };
    }

    return null;
  }

  public static integer(control: AbstractControl): ValidationErrors | null {
    const { value } = control;
    if (value === null || value === '') {
      return null;
    }

    return Number.isInteger(Number(value)) ? null : { integer: true };
  }
}
