import { Injectable } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { ObjectMapper } from '../../../shared/utils/object-mapper.adapter';
import type { MeResponse } from '../interfaces/me.interfaces';
import { MeService } from './me.service';

@Injectable({ providedIn: 'root' })
export class AccessStoreService {
  readonly #me$: Observable<MeResponse>;

  constructor(private readonly meSvc: MeService) {
    this.#me$ = this.meSvc.me().pipe(shareReplay({ bufferSize: 1, refCount: false }));
  }

  me$(): Observable<MeResponse> {
    return this.#me$;
  }

  hasPermission(me: MeResponse | null, code: string): boolean {
    return me?.permissionCodes?.some((p) => p === code) ? true : false;
  }

  freeze(me: MeResponse): Readonly<MeResponse> {
    return ObjectMapper.deepFreeze(me);
  }
}
