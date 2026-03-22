import { Component, ChangeDetectionStrategy, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ThemePickerComponent } from '../../../shared/components/theme-picker/theme-picker.component';
import { AuthService } from '../../../core/services/auth.service';
import { AuthStore } from '../../../core/store/auth.store';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ThemePickerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="login-page">
      <div class="login-theme-toggle">
        <app-theme-picker />
      </div>

      <div class="login-card animate-fade-up">
        <div class="login-logo">
          <svg width="40" height="40" viewBox="0 0 40 40" fill="none">
            <rect width="40" height="40" rx="12" fill="var(--color-primary-300)"/>
            <text x="9" y="28" fill="white" font-weight="800" font-size="22" font-family="var(--font-sans)">P</text>
          </svg>
        </div>

        <h1 class="heading-lg login-title">Bem-vindo ao PsicoFinance</h1>
        <p class="login-subtitle body-text">Faça login para acessar o sistema</p>

        <form class="login-form" (ngSubmit)="onLogin()">
          <div class="form-group">
            <label class="label-text" for="email">Email</label>
            <input
              class="input"
              id="email"
              type="email"
              placeholder="seu&#64;email.com"
              [(ngModel)]="email"
              name="email"
              required
            />
          </div>

          <div class="form-group">
            <label class="label-text" for="password">Senha</label>
            <input
              class="input"
              id="password"
              type="password"
              placeholder="Digite sua senha"
              [(ngModel)]="password"
              name="password"
              required
            />
          </div>

          <div class="login-options">
            <label class="login-checkbox">
              <input type="checkbox" [(ngModel)]="remember" name="remember" />
              <span class="body-text">Lembrar de mim</span>
            </label>
            <a routerLink="/auth/recuperar-senha" class="login-forgot body-text">Esqueceu a senha?</a>
          </div>

          @if (errorMessage()) {
            <div class="login-error body-text">{{ errorMessage() }}</div>
          }

          <button class="btn btn--primary login-btn" type="submit" [disabled]="loading()">
            {{ loading() ? 'Entrando...' : 'Entrar' }}
          </button>
        </form>

        <p class="login-footer caption-text">
          PsicoFinance &copy; 2026 — Gestão para clínicas de psicologia
        </p>
      </div>
    </div>
  `,
  styles: [`
    .login-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--color-bg);
      padding: 24px;
    }

    .login-theme-toggle {
      position: fixed;
      top: 16px;
      right: 16px;
      z-index: 100;
    }

    .login-card {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-2xl);
      padding: 40px 36px;
      width: 100%;
      max-width: 400px;
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .login-logo {
      margin-bottom: 20px;
    }

    .login-title {
      color: var(--color-text);
      text-align: center;
    }

    .login-subtitle {
      color: var(--color-muted);
      margin-top: 6px;
      margin-bottom: 28px;
    }

    .login-form {
      width: 100%;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 5px;

      label {
        color: var(--color-muted);
      }
    }

    .login-options {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .login-checkbox {
      display: flex;
      align-items: center;
      gap: 6px;
      cursor: pointer;

      input {
        accent-color: var(--color-primary-300);
      }

      span {
        color: var(--color-muted);
      }
    }

    .login-forgot {
      color: var(--color-primary-300);
      font-size: 12px;

      &:hover {
        color: var(--color-primary-400);
      }
    }

    .login-btn {
      width: 100%;
      padding: 10px;
      font-size: 14px;
      margin-top: 4px;
    }

    .login-error {
      background: #fef2f2;
      border: 1px solid #fecaca;
      color: #dc2626;
      padding: 10px 14px;
      border-radius: var(--radius-lg);
      font-size: 13px;
      text-align: center;
    }

    .login-footer {
      margin-top: 28px;
      color: var(--color-hint);
      text-align: center;
    }
  `],
})
export class LoginComponent {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  email = '';
  password = '';
  remember = false;
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  onLogin(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService
      .login({ email: this.email, senha: this.password })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMessage.set(
            err.status === 401
              ? 'Email ou senha inválidos.'
              : 'Erro ao fazer login. Tente novamente.'
          );
        },
      });
  }
}
