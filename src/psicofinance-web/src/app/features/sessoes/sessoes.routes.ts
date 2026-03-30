import { Routes } from '@angular/router';

export const SESSOES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./sessoes-list.component').then((m) => m.SessoesListComponent),
  },
  {
    path: 'agenda',
    loadComponent: () =>
      import('./sessoes-agenda.component').then((m) => m.SessoesAgendaComponent),
  },
  {
    path: 'nova',
    loadComponent: () =>
      import('./sessao-form.component').then((m) => m.SessaoFormComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./sessao-form.component').then((m) => m.SessaoFormComponent),
  },
];
