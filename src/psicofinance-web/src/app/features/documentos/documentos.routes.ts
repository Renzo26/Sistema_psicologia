import { Routes } from '@angular/router';

export const DOCUMENTOS_ROUTES: Routes = [
  {
    path: 'recibos',
    loadComponent: () =>
      import('./recibos/recibos-listagem.component').then(
        (m) => m.RecibosListagemComponent
      ),
  },
  {
    path: 'notas-fiscais',
    loadComponent: () =>
      import('./notas-fiscais/notas-fiscais.component').then(
        (m) => m.NotasFiscaisComponent
      ),
  },
  {
    path: 'relatorios',
    loadComponent: () =>
      import('./relatorios/relatorios-mensais.component').then(
        (m) => m.RelatoriosMensaisComponent
      ),
  },
  { path: '', redirectTo: 'recibos', pathMatch: 'full' },
];
