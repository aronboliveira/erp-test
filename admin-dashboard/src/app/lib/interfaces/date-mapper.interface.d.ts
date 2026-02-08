import type { TemporalKind } from '../types/temporal.types';

export interface DateMapper {
  getLastDay(d: Date): Date;
  getFirstDay(d: Date): Date;
  getLimitDays(d: Date): { first: Date; last: Date };
  getDaysLeft(a: Date, b: Date): number;
  getBusinessDaysLeft(a: Date, b: Date): number;
  isBusinessDay(d: Date): boolean;
  getMonthByNumber(d: Date): number;
  getWeekdayByDate(d: Date): number;
  getISOWeekday(d: Date): number;
  getISOWeekYear(d: Date): number;
  getISOWeekNumber(d: Date): number;
  getISOWeeksForYear(year: number): number;
  getWeekByNumber(d: Date): number;
  setAsWeekInput(stamp: string): string | null;
  setAsMonthInput(stamp: string): string | null;
}

export interface TemporalCandidate extends Readonly<{}> {
  raw: string;
  kind: TemporalKind;
  normalized: string;
}
