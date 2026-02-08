import type { TemporalKind } from '../../lib/types/temporal.types';

export class DateValidator {
  static RX = Object.freeze({
    'datetime-local': /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/,
    date: /^\d{4}-\d{2}-\d{2}$/,
    time: /^\d{2}:\d{2}$/,
    week: /^\d{4}-W\d{2}$/,
    month: /^\d{4}-\d{2}$/,
  } as const) satisfies Record<TemporalKind, RegExp>;

  public static toDate(kind: TemporalKind, v: string): Date {
    if (kind === 'time') return new Date(`1970-01-01T${v}:00`);
    if (kind === 'month') return new Date(`${v}-01T00:00`);
    if (kind === 'week') return new Date(`${v.slice(0, 4)}-01-01T00:00`);
    if (kind === 'date') return new Date(`${v}T00:00`);
    return new Date(v);
  }

  public static classifyTemporal(raw: string): { kind: TemporalKind; normalized: string } | null {
    for (const k of Object.keys(DateValidator.RX) as TemporalKind[])
      if (DateValidator.RX[k].test(raw)) return { kind: k, normalized: raw };
    return null;
  }
}
