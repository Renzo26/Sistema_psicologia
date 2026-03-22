import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'recuperar-senha',
    loadComponent: () =>
      import('./recuperar-senha/recuperar-senha.component').then(
        (m) => m.RecuperarSenhaComponent
      ),
  },
  {
    path: 'redefinir-senha',
    loadComponent: () =>
      import('./redefinir-senha/redefinir-senha.component').then(
        (m) => m.RedefinirSenhaComponent
      ),
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
];
