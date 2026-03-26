import { Routes } from '@angular/router';

export const CADASTROS_ROUTES: Routes = [
  {
    path: 'psicologos',
    loadComponent: () =>
      import('./psicologos/psicologos-list.component').then(
        (m) => m.PsicologosListComponent
      ),
  },
  {
    path: 'psicologos/novo',
    loadComponent: () =>
      import('./psicologos/psicologo-form.component').then(
        (m) => m.PsicologoFormComponent
      ),
  },
  {
    path: 'psicologos/:id',
    loadComponent: () =>
      import('./psicologos/psicologo-form.component').then(
        (m) => m.PsicologoFormComponent
      ),
  },
  {
    path: 'pacientes',
    loadComponent: () =>
      import('./pacientes/pacientes-list.component').then(
        (m) => m.PacientesListComponent
      ),
  },
  {
    path: 'pacientes/novo',
    loadComponent: () =>
      import('./pacientes/paciente-form.component').then(
        (m) => m.PacienteFormComponent
      ),
  },
  {
    path: 'pacientes/:id',
    loadComponent: () =>
      import('./pacientes/paciente-form.component').then(
        (m) => m.PacienteFormComponent
      ),
  },
  { path: '', redirectTo: 'psicologos', pathMatch: 'full' },
];
