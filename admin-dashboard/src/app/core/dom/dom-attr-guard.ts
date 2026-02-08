export class DomAttrGuard {
  static has(el: Element, k: string): boolean {
    return el.hasAttribute(`data-${k}`);
  }

  static get(el: Element, k: string): string | null {
    return el.getAttribute(`data-${k}`);
  }

  static set(el: Element, k: string, v: string): void {
    el.setAttribute(`data-${k}`, v);
  }
}
