export class RetryPolicy {
  readonly #tries: number;
  readonly #intervalMs: number;

  constructor(tries: number, intervalMs: number) {
    this.#tries = Number.isFinite(tries) && tries > 0 ? Math.floor(tries) : 5;
    this.#intervalMs = Number.isFinite(intervalMs) && intervalMs > 0 ? Math.floor(intervalMs) : 200;
  }

  get tries(): number {
    return this.#tries;
  }
  get intervalMs(): number {
    return this.#intervalMs;
  }
}
