export class DomBootCoordinator {
  static wireOnce(el: Element, markerAttr = 'data-wired', wire: () => void): void {
    if (!(el instanceof Element)) return;
    if (el.hasAttribute(markerAttr)) return;
    el.setAttribute(markerAttr, '1');
    try {
      wire();
    } catch (e) {
      console.error('wireOnce failed', e);
    }
  }

  static retryUntil(opts: {
    tries: number;
    delayMs: number;
    predicate: () => boolean;
    effect: () => void;
  }): void {
    const { tries, delayMs, predicate, effect } = opts;

    const step = (left: number) => {
      if (left <= 0) return;
      let ok = false;
      try {
        ok = predicate();
      } catch (_) {
        ok = false;
      }

      if (ok) {
        try {
          effect();
        } catch (e) {
          console.error('retryUntil effect failed', e);
        }
        return;
      }

      setTimeout(() => step(left - 1), delayMs);
    };

    step(tries);
  }

  static setAttrIfDiff(el: Element, name: string, value: string | null): void {
    if (!(el instanceof Element)) return;
    const current = el.getAttribute(name);

    if (value === null) {
      current !== null ? el.removeAttribute(name) : null;
      return;
    }

    current !== value ? el.setAttribute(name, value) : null;
  }
}
