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
  {
    path: 'fluxo-caixa',
    loadComponent: () =>
      import('./fluxo-caixa/fluxo-caixa.component').then(
        (m) => m.FluxoCaixaComponent
      ),
  },
  {
    path: 'fechamento',
    loadComponent: () =>
      import('./fechamento/fechamento-mensal.component').then(
        (m) => m.FechamentoMensalComponent
      ),
  },
  { path: '', redirectTo: 'lancamentos', pathMatch: 'full' },
];
