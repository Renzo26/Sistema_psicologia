import { Component, ChangeDetectionStrategy, inject, signal, DestroyRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ThemePickerComponent } from '../../../shared/components/theme-picker/theme-picker.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-redefinir-senha',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ThemePickerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="reset-page">
      <div class="reset-theme-toggle">
        <app-theme-picker />
      </div>

      <div class="reset-card animate-fade-up">
        <div class="reset-logo">
          <svg width="40" height="40" viewBox="0 0 40 40" fill="none">
            <rect width="40" height="40" rx="12" fill="var(--color-primary-300)"/>
            <text x="9" y="28" fill="white" font-weight="800" font-size="22" font-family="var(--font-sans)">P</text>
          </svg>
        </div>

        @if (!success()) {
          <h1 class="heading-lg reset-title">Redefinir Senha</h1>
          <p class="reset-subtitle body-text">Crie uma nova senha para sua conta.</p>

          <form class="reset-form" (ngSubmit)="onSubmit()">
            <div class="form-group">
              <label class="label-text" for="novaSenha">Nova Senha</label>
              <input
                class="input"
                id="novaSenha"
                type="password"
                placeholder="Mínimo 8 caracteres"
                [(ngModel)]="novaSenha"
                name="novaSenha"
                required
              />
            </div>

            <div class="form-group">
              <label class="label-text" for="confirmarSenha">Confirmar Senha</label>
              <input
                class="input"
                id="confirmarSenha"
                type="password"
                placeholder="Repita a nova senha"
                [(ngModel)]="confirmarSenha"
                name="confirmarSenha"
                required
              />
            </div>

            @if (errorMessage()) {
              <div class="reset-error body-text">{{ errorMessage() }}</div>
            }

            <button class="btn btn--primary reset-btn" type="submit" [disabled]="loading()">
              {{ loading() ? 'Salvando...' : 'Redefinir Senha' }}
            </button>
          </form>
        } @else {
          <h1 class="heading-lg reset-title">Senha Redefinida</h1>
          <p class="reset-subtitle body-text">Sua senha foi alterada com sucesso.</p>
          <div class="reset-success">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="var(--color-primary-300)" stroke-width="2">
              <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" stroke-linecap="round"/>
              <polyline points="22 4 12 14.01 9 11.01" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </div>
        }

        <a routerLink="/auth/login" class="reset-back body-text">
          ← Voltar para o login
        </a>

        <p class="reset-footer caption-text">
          PsicoFinance &copy; 2026 — Gestão para clínicas de psicologia
        </p>
      </div>
    </div>
  `,
  styles: [`
    .reset-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--color-bg);
      padding: 24px;
    }

    .reset-theme-toggle {
      position: fixed;
      top: 16px;
      right: 16px;
      z-index: 100;
    }

    .reset-card {
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

    .reset-logo { margin-bottom: 20px; }

    .reset-title { color: var(--color-text); text-align: center; }

    .reset-subtitle {
      color: var(--color-muted);
      margin-top: 6px;
      margin-bottom: 28px;
      text-align: center;
    }

    .reset-form {
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

    .reset-error {
      background: #fef2f2;
      border: 1px solid #fecaca;
      color: #dc2626;
      padding: 10px 14px;
      border-radius: var(--radius-lg);
      font-size: 13px;
      text-align: center;
    }

    .reset-success {
      margin: 16px 0 24px;
      display: flex;
      justify-content: center;
    }

    .reset-btn {
      width: 100%;
      padding: 10px;
      font-size: 14px;
      margin-top: 4px;
    }

    .reset-back {
      margin-top: 24px;
      color: var(--color-primary-300);
      font-size: 13px;
      &:hover { color: var(--color-primary-400); }
    }

    .reset-footer {
      margin-top: 20px;
      color: var(--color-hint);
      text-align: center;
    }
  `],
})
export class RedefinirSenhaComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private token = '';
  novaSenha = '';
  confirmarSenha = '';
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  success = signal(false);

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!this.token) {
      this.router.navigate(['/auth/login']);
    }
  }

  onSubmit(): void {
    if (this.novaSenha !== this.confirmarSenha) {
      this.errorMessage.set('As senhas não coincidem.');
      return;
    }

    if (this.novaSenha.length < 8) {
      this.errorMessage.set('A senha deve ter no mínimo 8 caracteres.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.authService
      .redefinirSenha(this.token, this.novaSenha)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.success.set(true);
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Token inválido ou expirado. Solicite uma nova recuperação.');
        },
      });
  }
}
