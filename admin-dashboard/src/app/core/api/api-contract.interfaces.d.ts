import type { HttpMethod, QueryOf, ReqOf, ResOf, RouteId } from './api-contract.types';

export interface ApiRouteRuntimeSpec {
  method: HttpMethod;
  path: string;
}

export interface ApiClientPort {
  call<I extends RouteId>(
    id: I,
    opts?: Readonly<{
      query?: QueryOf<I>;
      body?: ReqOf<I>;
      signal?: AbortSignal;
    }>,
  ): Promise<ResOf<I>>;
}
