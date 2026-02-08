import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { firstValueFrom, timer, Observable, fromEventPattern, NEVER, throwError } from 'rxjs';
import { catchError, retry, retryWhen, delay, scan, takeUntil } from 'rxjs/operators';

import type { ApiClientPort } from './api-contract.interfaces';
import type { HttpMethod, QueryOf, ReqOf, ResOf, RouteId } from './api-contract.types';
import { API_ROUTES } from './api-routes.const';
import { isPlatformBrowser } from '@angular/common';
import type {
  ApiMethod,
  ApiPath,
  ApiReq,
  ApiRes,
  BodyFor,
  HeadersFor,
  Jsonish,
  ReqFor,
} from './api.spec';
import ExceptionHandler from '../../shared/utils/exception-handler.util';

@Injectable({ providedIn: 'root' })
export class ApiClientService implements ApiClientPort {
  static readonly RETRY_COUNT = 3;
  static readonly RETRY_DELAY_MS = 250;
  static readonly _maxRetries = 3;
  readonly #isBrowser: boolean;

  constructor(
    private readonly http: HttpClient,
    @Inject(PLATFORM_ID) private readonly platformId: object,
  ) {
    this.#isBrowser = isPlatformBrowser(this.platformId);
  }

  async call<I extends RouteId>(
    id: I,
    opts?: Readonly<{ query?: QueryOf<I>; body?: ReqOf<I>; signal?: AbortSignal }>,
  ): Promise<ResOf<I>> {
    const spec = (API_ROUTES as any)[id] as { method: string; path: string } | undefined;
    if (!spec) throw new Error(`Unknown route: ${String(id)}`);

    const params = this.#toParams(opts?.query);
    const body = opts?.body as any;

    const req$ =
      spec.method === 'GET'
        ? this.http.get<ResOf<I>>(spec.path, { params })
        : spec.method === 'POST'
          ? this.http.post<ResOf<I>>(spec.path, body, { params })
          : this.http.request<ResOf<I>>(spec.method, spec.path, { body, params });

    const aborted$ = this.#aborted$(opts?.signal);

    const shouldRetry = this.#isIdempotentMethod(spec.method);

    return firstValueFrom(
      shouldRetry
        ? req$.pipe(
            takeUntil(aborted$),
            retry({
              count: ApiClientService.RETRY_COUNT,
              delay: () => timer(ApiClientService.RETRY_DELAY_MS),
            }),
          )
        : req$.pipe(takeUntil(aborted$)),
    );
  }

  public getMe(): Observable<ApiRes<'/api/me', 'GET'>> {
    return this.request('GET', '/api/me');
  }

  public request<TRes, M extends HttpMethod, TReq extends BodyFor<M> = BodyFor<M>>(
    method: M,
    url: string,
    body?: ReqFor<M, TReq>,
    headers?: HeadersFor<M>,
  ): Observable<TRes> {
    const h = this.#headers(headers);
    const opts = { headers: h };

    const req$ =
      method === 'GET'
        ? this.http.get<TRes>(url, opts)
        : method === 'DELETE'
          ? this.http.delete<TRes>(url, opts)
          : method === 'PUT'
            ? this.http.put<TRes>(url, body as any, opts)
            : this.http.post<TRes>(url, body as any, opts);

    if (!this.#isIdempotentMethod(method)) {
      return req$.pipe(catchError((e) => throwError(() => e)));
    }

    return req$.pipe(
      retryWhen((err$) =>
        err$.pipe(
          scan((acc) => {
            if (acc >= ApiClientService._maxRetries) throw new Error('api: max retries');
            return acc + 1;
          }, 0),
          delay(250),
        ),
      ),
      catchError((e) => throwError(() => e)),
    );
  }

  public get<TRes>(url: string, headers?: Record<string, string>) {
    return this.request<TRes, 'GET'>('GET', url, undefined, headers);
  }

  public post<TRes, TReq extends Record<string, any>>(
    url: string,
    body: TReq,
    headers?: Record<string, string>,
  ) {
    return this.request<TRes, 'POST', TReq>('POST', url, body, headers);
  }

  public isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  #headers(h?: Record<string, string> | void): HttpHeaders {
    let out = new HttpHeaders({ 'Content-Type': 'application/json' });
    if (!h) return out;
    for (const k of Object.keys(h)) out = out.set(k, String(h[k] ?? ''));
    return out;
  }

  #defaultHeaders(): HttpHeaders {
    return new HttpHeaders({
      Accept: 'application/json',
      'Content-Type': 'application/json',
    });
  }

  #resolveUrl(path: string): string {
    if (!path.startsWith('/')) path = `/${path}`;
    return path;
  }

  #mapError(e: unknown) {
    if (e instanceof HttpErrorResponse) {
      const friendly =
        ExceptionHandler.getHttpResponseFriendlyMessage(e.status) ||
        e.statusText ||
        'Request failed';
      return throwError(() => new Error(`${friendly} (${e.status})`));
    }
    const classified =
      e instanceof Error ? ExceptionHandler.classify(e) : { status: 500, type: 'Unknown' };
    return throwError(() => new Error(`${classified.type} (${classified.status})`));
  }

  #toParams<Q>(query: Q | undefined): HttpParams | undefined {
    if (!query) return undefined;

    let p = new HttpParams();
    for (const [k, v] of Object.entries(query as any)) {
      if (v !== undefined && v !== null) p = p.set(k, String(v));
    }
    return p;
  }

  #isIdempotentMethod(method: string): boolean {
    const m = method.toUpperCase();
    return m === 'GET' || m === 'HEAD' || m === 'PUT' || m === 'DELETE';
  }

  #aborted$(signal: AbortSignal | undefined): Observable<unknown> {
    if (!signal) return NEVER;
    if (signal.aborted) {
      // Emit immediately to cancel synchronously.
      return new Observable((sub) => {
        sub.next(true);
        sub.complete();
      });
    }
    return fromEventPattern<unknown>(
      (handler) => signal.addEventListener('abort', handler as any, { once: true }),
      (handler) => signal.removeEventListener('abort', handler as any),
    );
  }
}
