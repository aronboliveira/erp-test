export type FormId = string;
export type FieldKey = string;
export type FieldValue = string;

export type PersistedFormsState = Readonly<{
  [formId: FormId]: Readonly<Record<FieldKey, FieldValue>>;
}>;

export type PersistScope = 'local' | 'session';

export type PersistWriteMode = 'immediate' | 'debounced';

export type PersistConfig = Readonly<{
  scope: PersistScope;
  writeMode: PersistWriteMode;
  debounceMs: number;
  restoreTries: number;
  restoreDelayMs: number;
}>;
