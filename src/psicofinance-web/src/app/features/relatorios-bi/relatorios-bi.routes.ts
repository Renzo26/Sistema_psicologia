import { Routes } from '@angular/router';

export const RELATORIOS_BI_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./relatorios-lista/relatorios-lista.component').then(
        (m) => m.RelatoriosListaComponent
      ),
  },
  {
    path: 'novo',
    loadComponent: () =>
      import('./relatorio-editor/relatorio-editor.component').then(
        (m) => m.RelatorioEditorComponent
      ),
  },
  {
    path: ':id/editar',
    loadComponent: () =>
      import('./relatorio-editor/relatorio-editor.component').then(
        (m) => m.RelatorioEditorComponent
      ),
  },
  {
    path: ':id/visualizar',
    loadComponent: () =>
      import('./relatorio-visualizador/relatorio-visualizador.component').then(
        (m) => m.RelatorioVisualizadorComponent
      ),
  },
  {
    path: 'executar',
    loadComponent: () =>
      import('./relatorio-visualizador/relatorio-visualizador.component').then(
        (m) => m.RelatorioVisualizadorComponent
      ),
  },
];
