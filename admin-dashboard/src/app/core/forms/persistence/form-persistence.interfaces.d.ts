import type { PersistedFormsState, PersistScope } from './form-persistence.types';

export interface StoragePort {
  get(scope: PersistScope, key: string): string | null;
  set(scope: PersistScope, key: string, value: string): void;
  remove(scope: PersistScope, key: string): void;
}

export interface FormPersistencePort {
  snapshot(): PersistedFormsState;
  readForm(formId: string): Readonly<Record<string, string>> | null;
  patchField(formId: string, key: string, value: string): void;
  clearForm(formId: string): void;
}
