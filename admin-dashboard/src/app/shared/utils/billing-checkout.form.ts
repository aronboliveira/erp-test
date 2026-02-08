import { FormControl, FormGroup, Validators } from '@angular/forms';

export interface BillingCheckoutFormValue {
  plan: 'starter' | 'pro';
  email: string | null;
}

export interface BillingCheckoutForm {
  plan: FormControl<'starter' | 'pro'>;
  email: FormControl<string | null>;
}

export const createBillingCheckoutForm = () =>
  new FormGroup<BillingCheckoutForm>({
    plan: new FormControl<'starter' | 'pro'>('pro', { nonNullable: true }),
    email: new FormControl<string | null>(null, [Validators.email]),
  });
