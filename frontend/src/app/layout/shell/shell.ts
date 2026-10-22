import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.html',
  styleUrl: './shell.css',
  host: {
    '(document:keydown.escape)': 'closeMenu()'
  }
})
export class Shell {
  readonly auth = inject(AuthService);
  readonly menuOpen = signal(false);

  readonly menu = [
    { label: 'Dashboard', path: '/dashboard' },
    { label: 'Countries', path: '/countries' },
    { label: 'States', path: '/states' },
    { label: 'Cities', path: '/cities' },
    { label: 'Students', path: '/students' }
  ];

  openMenu(): void {
    this.menuOpen.set(true);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }
}