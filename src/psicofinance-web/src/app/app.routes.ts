import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: '',
    loadComponent: () =>
      import('./shared/layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent
      ),
    children: [
      {
        path: 'dashboard',
        loadChildren: () =>
          import('./features/dashboard/dashboard.routes').then(
            (m) => m.DASHBOARD_ROUTES
          ),
      },
      {
        path: 'cadastros',
        loadChildren: () =>
          import('./features/cadastros/cadastros.routes').then(
            (m) => m.CADASTROS_ROUTES
          ),
      },
      {
        path: 'sessoes',
        loadChildren: () =>
          import('./features/sessoes/sessoes.routes').then(
            (m) => m.SESSOES_ROUTES
          ),
      },
      {
        path: 'financeiro',
        loadChildren: () =>
          import('./features/financeiro/financeiro.routes').then(
            (m) => m.FINANCEIRO_ROUTES
          ),
      },
      {
        path: 'configuracoes',
        loadChildren: () =>
          import('./features/configuracoes/configuracoes.routes').then(
            (m) => m.CONFIGURACOES_ROUTES
          ),
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: 'auth/login' },
];
