import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { DashboardCounts, DashboardService } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  readonly auth = inject(AuthService);
  private readonly dashboardService = inject(DashboardService);

  readonly counts = signal<DashboardCounts | null>(null);

  readonly cards: { key: keyof DashboardCounts; label: string; link: string }[] = [
    { key: 'countries', label: 'Countries', link: '/countries' },
    { key: 'states', label: 'States', link: '/states' },
    { key: 'cities', label: 'Cities', link: '/cities' },
    { key: 'students', label: 'Students', link: '/students' }
  ];

  ngOnInit(): void {
    this.dashboardService.getCounts().subscribe((counts) => this.counts.set(counts));
  }

  countFor(key: keyof DashboardCounts): string {
    const counts = this.counts();
    if (!counts) {
      return '…';
    }
    const value = counts[key];
    return value === null ? '—' : String(value);
  }
}