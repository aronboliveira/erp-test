import type { SidebarConfig } from '../../lib/interfaces/sidebar.interfaces';
import type { SidebarItem } from '../../lib/types/sidebar.types';
import type { DashboardViewId } from '../../lib/types/dashboard-view.types';
import { ObjectMapper } from '../../shared/utils/object-mapper.adapter';

export const SIDEBAR_CONFIG: SidebarConfig<SidebarItem & { id: DashboardViewId }> =
  ObjectMapper.deepFreeze({
    sections: [
      {
        kind: 'section',
        id: 'sec.dashboard',
        label: 'Dashboard',
        defaultOpen: true,
        children: [
          { kind: 'item', id: 'revenues', label: 'Revenues', ariaLabel: 'Open revenues dashboard' },
          { kind: 'item', id: 'expenses', label: 'Expenses', ariaLabel: 'Open expenses dashboard' },
        ],
      },
      {
        kind: 'section',
        id: 'sec.configs',
        label: 'Configs',
        defaultOpen: true,
        children: [
          { kind: 'item', id: 'configs', label: 'Preferences', ariaLabel: 'Open configs' },
        ],
      },
    ],
  });
