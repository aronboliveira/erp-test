import { Directive, ElementRef, effect, OnDestroy, Renderer2 } from '@angular/core';
import { AppConfigsService } from '../../lib/shared/services/app-configs.service';
import { DomBootCoordinator } from '../utils/dom-boot-coordinator.util';

@Directive({
  selector: '[appDarkModeClass]',
  standalone: true,
})
export class DarkModeClassDirective implements OnDestroy {
  readonly #cls = 'dark-mode';
  readonly #markerAttr = 'data-darkmode-wired';

  readonly #ref: { destroy: () => void };

  constructor(elRef: ElementRef<HTMLElement>, r2: Renderer2, cfg: AppConfigsService) {
    const el = elRef.nativeElement;

    DomBootCoordinator.wireOnce(el, this.#markerAttr, () => {
      this.#apply(r2, el, cfg.isDarkMode());
    });

    this.#ref = effect(() => this.#apply(r2, el, cfg.isDarkMode()));
  }

  ngOnDestroy(): void {
    this.#ref?.destroy();
  }

  #apply(r2: Renderer2, el: HTMLElement, enabled: boolean): void {
    if (!el) return;

    const has = el.classList.contains(this.#cls);

    enabled && !has ? r2.addClass(el, this.#cls) : null;
    !enabled && has ? r2.removeClass(el, this.#cls) : null;
  }
}
