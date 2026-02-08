import { DashboardViewId } from '../types/dashboard-view.types';
import type { Flag01 } from '../types/flags.types';

export interface AppConfigs {
  dark_mode: Flag01;
  last_view: DashboardViewId;
}
