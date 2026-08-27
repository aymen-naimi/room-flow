import { FormControl } from '@angular/forms';
import { FormValidators } from './form-validators';

describe('FormValidators', () => {
  describe('requiredText', () => {
    it('rejects empty and whitespace-only values', () => {
      expect(FormValidators.requiredText(new FormControl(''))).toEqual({ required: true });
      expect(FormValidators.requiredText(new FormControl('   '))).toEqual({ required: true });
    });

    it('accepts a non-blank string', () => {
      expect(FormValidators.requiredText(new FormControl('Horizon'))).toBeNull();
    });
  });

  describe('integer', () => {
    it('rejects a decimal', () => {
      expect(FormValidators.integer(new FormControl(1.5))).toEqual({ integer: true });
    });

    it('accepts an integer and leaves empty to required', () => {
      expect(FormValidators.integer(new FormControl(8))).toBeNull();
      expect(FormValidators.integer(new FormControl(null))).toBeNull();
    });
  });
});
