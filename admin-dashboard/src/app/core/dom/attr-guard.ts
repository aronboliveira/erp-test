export class AttrGuard {
  static has(el: Element, attr: string): boolean {
    return el.hasAttribute(attr);
  }

  static get(el: Element, attr: string): string | null {
    return el.getAttribute(attr);
  }

  static setIfChanged(el: Element, attr: string, value: string): void {
    const prev = AttrGuard.get(el, attr);
    prev === value ? void 0 : el.setAttribute(attr, value);
  }
}
