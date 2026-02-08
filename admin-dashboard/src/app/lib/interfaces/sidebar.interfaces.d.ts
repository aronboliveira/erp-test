import type { SidebarItem, SidebarSection } from '../types/sidebar.types';

export interface SidebarConfig<TItem extends SidebarItem = SidebarItem> {
  readonly sections: readonly SidebarSection<TItem>[];
}

export interface SidebarSelection<TItem extends SidebarItem = SidebarItem> {
  readonly item: TItem;
}
