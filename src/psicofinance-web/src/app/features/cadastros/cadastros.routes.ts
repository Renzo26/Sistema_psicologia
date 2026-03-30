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
  {
    path: 'contratos',
    loadComponent: () =>
      import('./contratos/contratos-list.component').then(
        (m) => m.ContratosListComponent
      ),
  },
  {
    path: 'contratos/novo',
    loadComponent: () =>
      import('./contratos/contrato-form.component').then(
        (m) => m.ContratoFormComponent
      ),
  },
  {
    path: 'contratos/:id',
    loadComponent: () =>
      import('./contratos/contrato-form.component').then(
        (m) => m.ContratoFormComponent
      ),
  },
  {
    path: 'planos-conta',
    loadComponent: () =>
      import('./planos-conta/planos-conta.component').then(
        (m) => m.PlanosContaComponent
      ),
  },
  { path: '', redirectTo: 'psicologos', pathMatch: 'full' },
];
