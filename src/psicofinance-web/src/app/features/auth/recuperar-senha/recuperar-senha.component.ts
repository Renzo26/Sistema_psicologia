import { Component, ChangeDetectionStrategy, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ThemePickerComponent } from '../../../shared/components/theme-picker/theme-picker.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-recuperar-senha',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ThemePickerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="recover-page">
      <div class="recover-theme-toggle">
        <app-theme-picker />
      </div>

      <div class="recover-card animate-fade-up">
        <div class="recover-logo">
          <svg width="40" height="40" viewBox="0 0 40 40" fill="none">
            <rect width="40" height="40" rx="12" fill="var(--color-primary-300)"/>
            <text x="9" y="28" fill="white" font-weight="800" font-size="22" font-family="var(--font-sans)">P</text>
          </svg>
        </div>

        @if (!emailSent()) {
          <h1 class="heading-lg recover-title">Recuperar Senha</h1>
          <p class="recover-subtitle body-text">
            Informe seu email e enviaremos instruções para redefinir sua senha.
          </p>

          <form class="recover-form" (ngSubmit)="onSubmit()">
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

            @if (errorMessage()) {
              <div class="recover-error body-text">{{ errorMessage() }}</div>
            }

            <button class="btn btn--primary recover-btn" type="submit" [disabled]="loading()">
              {{ loading() ? 'Enviando...' : 'Enviar link de recuperação' }}
            </button>
          </form>
        } @else {
          <h1 class="heading-lg recover-title">Email Enviado</h1>
          <p class="recover-subtitle body-text">
            Se o email informado estiver cadastrado, você receberá um link para redefinir sua senha.
          </p>
          <div class="recover-success">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="var(--color-primary-300)" stroke-width="2">
              <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" stroke-linecap="round"/>
              <polyline points="22 4 12 14.01 9 11.01" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </div>
        }

        <a routerLink="/auth/login" class="recover-back body-text">
          ← Voltar para o login
        </a>

        <p class="recover-footer caption-text">
          PsicoFinance &copy; 2026 — Gestão para clínicas de psicologia
        </p>
      </div>
    </div>
  `,
  styles: [`
    .recover-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--color-bg);
      padding: 24px;
    }

    .recover-theme-toggle {
      position: fixed;
      top: 16px;
      right: 16px;
      z-index: 100;
    }

    .recover-card {
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

    .recover-logo { margin-bottom: 20px; }

    .recover-title {
      color: var(--color-text);
      text-align: center;
    }

    .recover-subtitle {
      color: var(--color-muted);
      margin-top: 6px;
      margin-bottom: 28px;
      text-align: center;
    }

    .recover-form {
      width: 100%;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 5px;

      label { color: var(--color-muted); }
    }

    .recover-error {
      background: #fef2f2;
      border: 1px solid #fecaca;
      color: #dc2626;
      padding: 10px 14px;
      border-radius: var(--radius-lg);
      font-size: 13px;
      text-align: center;
    }

    .recover-success {
      margin: 16px 0 24px;
      display: flex;
      justify-content: center;
    }

    .recover-btn {
      width: 100%;
      padding: 10px;
      font-size: 14px;
      margin-top: 4px;
    }

    .recover-back {
      margin-top: 24px;
      color: var(--color-primary-300);
      font-size: 13px;

      &:hover { color: var(--color-primary-400); }
    }

    .recover-footer {
      margin-top: 20px;
      color: var(--color-hint);
      text-align: center;
    }
  `],
})
export class RecuperarSenhaComponent {
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  email = '';
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  emailSent = signal(false);

  onSubmit(): void {
    if (!this.email) {
      this.errorMessage.set('Informe seu email.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService
      .solicitarRecuperacao(this.email)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.emailSent.set(true);
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Erro ao enviar email. Tente novamente.');
        },
      });
  }
}
