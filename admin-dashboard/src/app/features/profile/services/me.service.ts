import { Injectable } from '@angular/core';
import { ApiClientService } from '../../../core/api/api-client.service';
import { ApiRoutes } from '../../../core/api/api.routes';
import type { MeResponse } from '../interfaces/me.interfaces';

@Injectable({ providedIn: 'root' })
export class MeService {
  constructor(private readonly api: ApiClientService) {}

  me() {
    return this.api.get<MeResponse>(ApiRoutes.me);
  }
}
