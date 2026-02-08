import { AttrGuard } from './attr-guard';

export class DatasetGuard {
  static readonly #prefix = 'data-admin-';

  static has(el: Element, key: string): boolean {
    return AttrGuard.has(el, `${DatasetGuard.#prefix}${key}`);
  }

  static mark(el: Element, key: string, value = '1'): void {
    const attr = `${DatasetGuard.#prefix}${key}`;
    AttrGuard.has(el, attr) ? void 0 : el.setAttribute(attr, value);
  }

  static once(el: Element, key: string, fn: () => void): void {
    DatasetGuard.has(el, key) ? void 0 : (DatasetGuard.mark(el, key), fn());
  }
}
