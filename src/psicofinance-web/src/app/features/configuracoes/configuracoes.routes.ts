import { Routes } from '@angular/router';

export const CONFIGURACOES_ROUTES: Routes = [
  {
    path: 'clinica',
    loadComponent: () =>
      import('./clinica/clinica-config.component').then(
        (m) => m.ClinicaConfigComponent
      ),
  },
  { path: '', redirectTo: 'clinica', pathMatch: 'full' },
];
