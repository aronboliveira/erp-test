import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-profile-menu',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-profile-menu.component.html',
  styleUrl: './user-profile-menu.component.scss',
})
export class UserProfileMenuComponent {
  @Input() displayName: string | null = 'Admin';
  @Input() email: string | null = 'admin@example.com';

  @Output() openProfile = new EventEmitter<void>();

  get initials(): string {
    const s = (this.displayName ?? '').trim();
    if (!s) return '?';
    const parts = s.split(/\s+/g).filter(Boolean);
    const a = parts[0]?.charAt(0) ?? '?';
    const b = parts.length > 1 ? parts[parts.length - 1].charAt(0) : '';
    return (a + b).toUpperCase();
  }
}
