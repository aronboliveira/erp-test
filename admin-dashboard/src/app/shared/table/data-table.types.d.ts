export type Primitive = string | number | boolean | null | undefined;

export type PathOf<T> = T extends Primitive
  ? never
  : T extends readonly any[]
    ? never
    : {
        [K in Extract<keyof T, string>]: T[K] extends Primitive
          ? `${K}`
          : T[K] extends object
            ? `${K}` | `${K}.${PathOf<T[K]>}`
            : `${K}`;
      }[Extract<keyof T, string>];

export type ValueAtPath<T, P extends string> = P extends `${infer H}.${infer R}`
  ? H extends keyof T
    ? ValueAtPath<T[H], R>
    : never
  : P extends keyof T
    ? T[P]
    : never;

export type ColumnDef<T> =
  | Readonly<{
      id: string;
      header: string;
      kind: 'path';
      path: PathOf<T>;
      ariaLabel?: string;
    }>
  | Readonly<{
      id: string;
      header: string;
      kind: 'render';
      ariaLabel?: string;
      render: (row: T) => string;
    }>;
