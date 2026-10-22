import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { Shell } from './layout/shell/shell';
import { Login } from './pages/login/login';
import { Dashboard } from './pages/dashboard/dashboard';
import { CountryList } from './pages/countries/country-list/country-list';
import { CountryForm } from './pages/countries/country-form/country-form';
import { StateList } from './pages/states/state-list/state-list';
import { StateForm } from './pages/states/state-form/state-form';

export const routes: Routes = [
  { path: 'login', component: Login, canActivate: [guestGuard], title: 'Sign in | ISMEDU' },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: 'dashboard', component: Dashboard, title: 'Dashboard | ISMEDU' },
      { path: 'countries', component: CountryList, title: 'Countries | ISMEDU' },
      { path: 'countries/new', component: CountryForm, title: 'Add country | ISMEDU' },
      { path: 'countries/:id/edit', component: CountryForm, title: 'Edit country | ISMEDU' },
      { path: 'states', component: StateList, title: 'States | ISMEDU' },
      { path: 'states/new', component: StateForm, title: 'Add state | ISMEDU' },
      { path: 'states/:id/edit', component: StateForm, title: 'Edit state | ISMEDU' },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];