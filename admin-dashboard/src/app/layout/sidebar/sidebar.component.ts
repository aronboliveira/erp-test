import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Inject,
  Input,
  Optional,
  Output,
  PLATFORM_ID,
  ViewChild,
  signal,
} from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { UserProfileMenuComponent } from './user-profile-menu/user-profile-menu.component';
import type { SidebarConfig, SidebarSelection } from '../../lib/interfaces/sidebar.interfaces';
import type {
  SidebarEntry,
  SidebarGroup,
  SidebarItem,
  SidebarSection,
} from '../../lib/types/sidebar.types';

import { DomBootCoordinator } from '../../shared/utils/dom-boot-coordinator.util';
import { DarkModeClassDirective } from '../../shared/directives/dark-mode-class.directive';
import { SIDEBAR_CONFIG } from './sidebar.config';
import { Router, RouterModule } from '@angular/router';
import { LocalDateMapper } from '../../core/dates/local-date-mapper.adapter';
import { SwalLoaderService } from '../../core/ui/swal-loader.service';
import { MeService } from '../../features/profile/services/me.service';
import { DateValidator } from '../../core/dates/date-validator.adapter';
import { firstValueFrom } from 'rxjs/internal/firstValueFrom';
import { AccessStoreService } from '../../features/profile/services/access-store.service';
import { shareReplay } from 'rxjs/internal/operators/shareReplay';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, DarkModeClassDirective, UserProfileMenuComponent, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent implements AfterViewInit {
  @Output() readonly select = new EventEmitter<SidebarSelection>();
  @Output() readonly collapsedChange = new EventEmitter<boolean>();
  readonly displayName: string | null = null;
  readonly email: string | null = null;
  @Input()
  set activeId(v: string | null) {
    this.activeItemId.set(v);
  }

  @ViewChild('root', { static: true }) private readonly rootRef?: ElementRef<HTMLElement>;

  readonly config: SidebarConfig = SIDEBAR_CONFIG;
  readonly #dt = new LocalDateMapper();

  readonly collapsed = signal(false);
  readonly mobileOpen = signal(false);
  readonly openedSections = signal<ReadonlySet<string>>(this.#initialOpenSet(this.config.sections));

  readonly activeItemId = signal<string | null>('revenues');
  readonly helpDrawerOpen = signal(false);
  readonly chatboxOpen = signal(false);

  readonly #isBrowser: boolean;
  #autoCollapse = true;
  readonly me$;

  constructor(
    @Inject(PLATFORM_ID) platformId: object,
    private readonly swal: SwalLoaderService,
    private readonly meSvc: MeService,
    private readonly access: AccessStoreService,
    @Optional() private readonly router?: Router,
  ) {
    this.#isBrowser = isPlatformBrowser(platformId);
    this.me$ = this.access.me$().pipe(shareReplay({ bufferSize: 1, refCount: true }));
  }

  can(user: any, permission: string): boolean {
    return user?.permissionCodes?.includes(permission) ?? false;
  }

  async openProfile(): Promise<void> {
    if (!this.#isBrowser) return;

    const Swal = (await this.swal.load()).default;

    let payload: any = null;
    try {
      payload = await firstValueFrom(this.meSvc.me());
    } catch {
      await Swal.fire({ icon: 'error', title: 'Profile', text: 'Failed to load /api/me' });
      return;
    }

    const roles =
      (payload?.roleNames ?? [])
        .map((r: string) => `<li>${this.#escapeHtml(r)}</li>`)
        .join('') || '<li>—</li>';
    const perms =
      (payload?.permissionCodes ?? [])
        .map((p: string) => `<li>${this.#escapeHtml(p)}</li>`)
        .join('') || '<li>—</li>';

    await Swal.fire({
      title: payload?.displayName ? `${this.#escapeHtml(payload.displayName)}` : 'User',
      html: `
        <div class="profile-modal">
          <div class="profile-modal__line"><strong>Email:</strong> ${
            payload?.email ? this.#escapeHtml(payload.email) : '—'
          }</div>
          <div class="profile-modal__grid">
            <section aria-label="Roles"><h3>Roles</h3><ul>${roles}</ul></section>
            <section aria-label="Permissions"><h3>Permissions</h3><ul>${perms}</ul></section>
          </div>
        </div>
      `,
      confirmButtonText: 'Close',
      focusConfirm: true,
    });
  }

  ngAfterViewInit(): void {
    if (!this.#isBrowser) return;
    const root = this.rootRef?.nativeElement;
    if (!root) return;

    if (this.#autoCollapse) {
      const w = window.innerWidth;
      this.collapsed.set(w < 1024);
    }
    this.collapsedChange.emit(this.collapsed());

    DomBootCoordinator.wireOnce(root, 'data-wired', () => this.#wireGlobalShortcuts(root));
    DomBootCoordinator.retryUntil({
      tries: 12,
      delayMs: 120,
      predicate: () => document.readyState === 'interactive' || document.readyState === 'complete',
      effect: () => this.#ensureAriaDefaults(root),
    });
  }

  toggleCollapsed(): void {
    this.collapsed.update((v) => !v);
    this.#autoCollapse = false;
    this.collapsedChange.emit(this.collapsed());
  }

  toggleMobile(): void {
    this.mobileOpen.update((v) => !v);
  }

  closeMobile(): void {
    this.mobileOpen() ? this.mobileOpen.set(false) : null;
  }

  toggleHelpDrawer(): void {
    this.helpDrawerOpen.update((v) => !v);
  }

  toggleChatbox(): void {
    this.chatboxOpen.update((v) => !v);
  }

  isSectionOpen(id: string): boolean {
    return this.openedSections().has(id);
  }

  toggleSection(id: string): void {
    const prev = this.openedSections();
    const next = new Set(prev);

    next.has(id) ? next.delete(id) : next.add(id);
    this.openedSections.set(Object.freeze(next));
  }

  onItemSelect(item: SidebarItem): void {
    this.activeItemId.set(item.id);
    this.select.emit({ item });
    this.router ? void this.router.navigateByUrl(`/${item.id}`) : null;

    this.mobileOpen() ? this.closeMobile() : null;
  }

  isGroup(entry: SidebarEntry): entry is SidebarGroup {
    return (entry as any)?.kind === 'group';
  }

  isItem(entry: SidebarEntry): entry is SidebarItem {
    return (entry as any)?.kind === 'item';
  }

  trackSection = (_: number, s: SidebarSection) => s.id;
  trackEntry = (_: number, e: SidebarEntry) => (e as any).id;

  #initialOpenSet(sections: readonly SidebarSection[]): ReadonlySet<string> {
    const s = new Set<string>();
    for (const sec of sections) sec.defaultOpen ? s.add(sec.id) : null;
    return Object.freeze(s);
  }

  #wireGlobalShortcuts(root: HTMLElement): void {
    const onKeyDown = (ev: KeyboardEvent) => {
      ev?.key === 'Escape' && this.mobileOpen() ? this.closeMobile() : null;
    };

    const onResize = () => {
      const w = window.innerWidth;
      w >= 1024 && this.mobileOpen() ? this.closeMobile() : null;
      if (this.#autoCollapse) {
        const next = w < 1024;
        if (next !== this.collapsed()) {
          this.collapsed.set(next);
          this.collapsedChange.emit(this.collapsed());
        }
      }
    };

    const kAttr = 'data-keydown-wired';
    const rAttr = 'data-resize-wired';

    !document.documentElement.hasAttribute(kAttr)
      ? (document.documentElement.setAttribute(kAttr, '1'),
        window.addEventListener('keydown', onKeyDown, { passive: true }))
      : null;

    !document.documentElement.hasAttribute(rAttr)
      ? (document.documentElement.setAttribute(rAttr, '1'),
        window.addEventListener('resize', onResize, { passive: true }))
      : null;

    DomBootCoordinator.setAttrIfDiff(root, 'aria-label', 'Dashboard navigation');
  }

  #ensureAriaDefaults(root: HTMLElement): void {
    const nav = root.querySelector('nav');
    nav ? DomBootCoordinator.setAttrIfDiff(nav, 'aria-label', 'Primary') : null;
  }

  #escapeHtml(value: string): string {
    return String(value)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }
}
