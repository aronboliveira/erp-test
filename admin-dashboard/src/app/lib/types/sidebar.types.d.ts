export type SidebarId = string;

export type SidebarItem = Readonly<{
  kind: 'item';
  id: SidebarId;
  label: string;
  ariaLabel?: string;
  badge?: string;
}>;

export type SidebarGroup<TItem extends SidebarItem = SidebarItem> = Readonly<{
  kind: 'group';
  id: SidebarId;
  label: string;
  children: readonly SidebarEntry<TItem>[];
}>;

export type SidebarSection<TItem extends SidebarItem = SidebarItem> = Readonly<{
  kind: 'section';
  id: SidebarId;
  label: string;
  defaultOpen?: boolean;
  children: readonly SidebarEntry<TItem>[];
}>;

export type SidebarEntry<TItem extends SidebarItem = SidebarItem> = TItem | SidebarGroup<TItem>;

/** Recursive helper (useful when you later add route metadata) */
export type SidebarWalk<TItem extends SidebarItem = SidebarItem> =
  | SidebarSection<TItem>
  | SidebarEntry<TItem>;

/** Find a node by id (compile-time aid for config refactors) */
export type SidebarFindById<TNode extends SidebarWalk<any>, TId extends string> = TNode extends {
  id: infer I;
}
  ? I extends TId
    ? TNode
    : TNode extends { children: readonly (infer C)[] }
      ? SidebarFindById<C & SidebarWalk<any>, TId>
      : never
  : never;
