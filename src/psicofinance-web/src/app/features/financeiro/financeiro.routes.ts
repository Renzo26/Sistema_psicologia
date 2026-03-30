import { Routes } from '@angular/router';

export const FINANCEIRO_ROUTES: Routes = [
  {
    path: 'lancamentos',
    loadComponent: () =>
      import('./lancamentos/lancamentos.component').then(
        (m) => m.LancamentosComponent
      ),
  },
  {
    path: 'repasses',
    loadComponent: () =>
      import('./repasses/repasses.component').then(
        (m) => m.RepassesComponent
      ),
  },
  { path: '', redirectTo: 'lancamentos', pathMatch: 'full' },
];
