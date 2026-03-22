import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { AuthStore, AuthUser } from '../store/auth.store';

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface AuthResponse {
  usuarioId: string;
  nome: string;
  email: string;
  role: 'Admin' | 'Gerente' | 'Secretaria' | 'Psicologo';
  clinicaId: string;
  accessToken: string;
}

export interface TrocarSenhaRequest {
  senhaAtual: string;
  novaSenha: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly authStore = inject(AuthStore);

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('auth/login', request).pipe(
      tap((response) => {
        const user: AuthUser = {
          id: response.usuarioId,
          nome: response.nome,
          email: response.email,
          role: response.role.toLowerCase() as AuthUser['role'],
          clinicaId: response.clinicaId,
        };
        this.authStore.loginSuccess(user, response.accessToken);
      })
    );
  }

  refresh(): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('auth/refresh', {}).pipe(
      tap((response) => {
        this.authStore.setAccessToken(response.accessToken);
      })
    );
  }

  logout(): Observable<void> {
    return this.api.post<void>('auth/logout', {}).pipe(
      tap(() => this.authStore.logout())
    );
  }

  trocarSenha(request: TrocarSenhaRequest): Observable<void> {
    return this.api.post<void>('auth/trocar-senha', request);
  }

  solicitarRecuperacao(email: string): Observable<void> {
    return this.api.post<void>('auth/recuperar-senha', { email });
  }

  redefinirSenha(token: string, novaSenha: string): Observable<void> {
    return this.api.post<void>('auth/redefinir-senha', { token, novaSenha });
  }
}
