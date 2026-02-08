import type { MeDto } from '../../lib/interfaces/me.interface';

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

type BodyFor<M extends HttpMethod> = M extends 'GET' | 'DELETE' ? void : Record<string, any>;

type HeadersFor<M extends HttpMethod> = M extends 'POST' | 'PUT'
  ? Record<string, string>
  : Record<string, string> | void;

type ReqFor<M extends HttpMethod, T> = M extends 'GET' | 'DELETE' ? void : T;

export interface ApiSpec {
  '/api/me': {
    GET: {
      res: MeDto;
      req?: never;
    };
  };
}

export type ApiPath = keyof ApiSpec;

export type ApiMethod<P extends ApiPath> = keyof ApiSpec[P] & HttpMethod;

export type ApiReq<P extends ApiPath, M extends ApiMethod<P>> = ApiSpec[P][M] extends {
  req: infer R;
}
  ? R
  : never;

export type ApiRes<P extends ApiPath, M extends ApiMethod<P>> = ApiSpec[P][M] extends {
  res: infer R;
}
  ? R
  : never;

export type Jsonish =
  | null
  | boolean
  | number
  | string
  | readonly Jsonish[]
  | { readonly [k: string]: Jsonish };
