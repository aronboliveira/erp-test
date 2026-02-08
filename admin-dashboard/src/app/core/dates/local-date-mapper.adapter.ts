import type { DateMapper } from '../../lib/interfaces/date-mapper.interface';
import { DateValidator } from './date-validator.adapter';

export class LocalDateMapper implements DateMapper {
  getLastDay(d: Date): Date {
    const y = d.getFullYear();
    const m = d.getMonth();
    const last = this.#daysInMonth(y, m);
    return new Date(y, m, last);
  }

  getFirstDay(d: Date): Date {
    return new Date(d.getFullYear(), d.getMonth(), 1);
  }

  getLimitDays(d: Date): { first: Date; last: Date } {
    return { first: this.getFirstDay(d), last: this.getLastDay(d) };
  }

  getDaysLeft(a: Date, b: Date): number {
    const A = this.#toMidnight(a);
    const B = this.#toMidnight(b);
    const diff = Math.floor((B.getTime() - A.getTime()) / 86400000);
    return diff > 0 ? diff : 0;
  }

  getBusinessDaysLeft(a: Date, b: Date): number {
    const total = this.getDaysLeft(a, b);
    if (total === 0) return 0;
    const cursor = this.#toMidnight(a);
    let count = 0;
    for (let i = 0; i < total; i++) {
      cursor.setDate(cursor.getDate() + 1);
      if (this.isBusinessDay(cursor)) count++;
    }
    return count;
  }

  isBusinessDay(d: Date): boolean {
    const wd = this.getWeekdayByDate(d);
    return wd >= 1 && wd <= 5;
  }

  getMonthByNumber(d: Date): number {
    return d.getMonth() + 1;
  }

  getWeekdayByDate(d: Date): number {
    return d.getDay();
  }

  getISOWeekday(d: Date): number {
    const day = d.getDay();
    return day === 0 ? 7 : day;
  }

  getISOWeekYear(date: Date): number {
    const d = this.#toMidnight(date);
    const wd = this.getISOWeekday(d);
    d.setDate(d.getDate() + (4 - wd));
    return d.getFullYear();
  }

  getISOWeekNumber(date: Date): number {
    const d = this.#toMidnight(date);
    const wd = this.getISOWeekday(d);
    d.setDate(d.getDate() + (4 - wd));
    const yearStart = new Date(d.getFullYear(), 0, 1);
    const diffDays = Math.floor((d.getTime() - yearStart.getTime()) / 86400000);
    return Math.floor(diffDays / 7) + 1;
  }

  getISOWeeksForYear(year: number): number {
    const jan1 = new Date(year, 0, 1);
    const wd = this.getISOWeekday(jan1);
    if (wd === 4) return 53;
    if (wd === 3 && this.#isLeapYear(year)) return 53;
    return 52;
  }

  getWeekByNumber(d: Date): number {
    return this.getISOWeekNumber(d);
  }

  setAsWeekInput(stamp: string): string | null {
    const parsed = DateValidator.classifyTemporal(stamp);
    if (!parsed) return null;
    if (parsed.kind !== 'date' && parsed.kind !== 'datetime-local') return null;
    const d = DateValidator.toDate(parsed.kind, parsed.normalized);
    if (Number.isNaN(d.getTime())) return null;
    const year = this.getISOWeekYear(d);
    const week = this.getISOWeekNumber(d);
    return `${year}-W${String(week).padStart(2, '0')}`;
  }

  setAsMonthInput(stamp: string): string | null {
    const parsed = DateValidator.classifyTemporal(stamp);
    if (!parsed) return null;
    if (parsed.kind !== 'date' && parsed.kind !== 'datetime-local') return null;
    const d = DateValidator.toDate(parsed.kind, parsed.normalized);
    if (Number.isNaN(d.getTime())) return null;
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }

  #daysInMonth(year: number, month: number): number {
    return new Date(year, month + 1, 0).getDate();
  }

  #toMidnight(d: Date): Date {
    return new Date(d.getFullYear(), d.getMonth(), d.getDate());
  }

  #isLeapYear(year: number): boolean {
    return (year % 4 === 0 && year % 100 !== 0) || year % 400 === 0;
  }
}
