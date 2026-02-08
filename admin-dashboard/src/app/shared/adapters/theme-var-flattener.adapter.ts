import type { CssVars } from '../../lib/types/theme.types';
import { ObjectMapper } from '../utils/object-mapper.adapter';

export class ThemeVarFlattener {
  static readonly #prefix = 'app';
  static readonly #rxCaps = /([a-z0-9])([A-Z])/g;

  public static toCssVars(tokens: object): CssVars {
    const out: Record<string, string> = {};
    this.#walk(tokens, '', out);

    const casted = out as any;
    return ObjectMapper.deepFreeze(casted);
  }

  static #walk(node: any, path: string, out: Record<string, string>): void {
    if (!node || typeof node !== 'object') return;

    if (Array.isArray(node)) {
      for (let i = 0; i < node.length; i++) {
        const v = node[i];
        typeof v === 'string'
          ? (out[this.#varName(`${path}-${i}`)] = v)
          : this.#walk(v, `${path}-${i}`, out);
      }
      return;
    }

    for (const k of Object.keys(node)) {
      const nextPath = path ? `${path}-${k}` : k;
      const v = node[k];

      typeof v === 'string' || typeof v === 'number'
        ? (out[this.#varName(nextPath)] = String(v))
        : this.#walk(v, nextPath, out);
    }
  }

  static #varName(path: string): `--app-${string}` {
    const kebab = path
      .replace(this.#rxCaps, '$1-$2')
      .replace(/[\s_]+/g, '-')
      .replace(/-+/g, '-')
      .toLowerCase();

    return `--${this.#prefix}-${kebab}`;
  }
}
