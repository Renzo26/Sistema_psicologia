import { Component, ChangeDetectionStrategy, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ThemePickerComponent } from '../../../shared/components/theme-picker/theme-picker.component';
import { ApiService } from '../../../core/services/api.service';
import { AuthStore } from '../../../core/store/auth.store';

interface OnboardingResponse {
  clinicaId: string;
  usuarioId: string;
  nomeClinica: string;
  nomeUsuario: string;
  email: string;
  accessToken: string;
}

@Component({
  selector: 'app-onboarding',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ThemePickerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="onboarding-page">
      <div class="onboarding-theme-toggle">
        <app-theme-picker />
      </div>

      <div class="onboarding-container">
        <div class="onboarding-header">
          <div class="onboarding-logo">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/>
            </svg>
          </div>
          <h1>PsicoFinance</h1>
          <p class="onboarding-subtitle">Configure sua clínica em menos de 5 minutos</p>
        </div>

        <!-- Progress Steps -->
        <div class="onboarding-steps">
          <div class="step" [class.active]="currentStep() >= 1" [class.completed]="currentStep() > 1">
            <div class="step-circle">1</div>
            <span>Clínica</span>
          </div>
          <div class="step-line" [class.active]="currentStep() > 1"></div>
          <div class="step" [class.active]="currentStep() >= 2" [class.completed]="currentStep() > 2">
            <div class="step-circle">2</div>
            <span>Admin</span>
          </div>
          <div class="step-line" [class.active]="currentStep() > 2"></div>
          <div class="step" [class.active]="currentStep() >= 3">
            <div class="step-circle">3</div>
            <span>Pronto!</span>
          </div>
        </div>

        @if (errorMessage()) {
          <div class="onboarding-error">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/>
              <line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
            {{ errorMessage() }}
          </div>
        }

        <!-- Step 1: Dados da Clínica -->
        @if (currentStep() === 1) {
          <form class="onboarding-form" (ngSubmit)="nextStep()">
            <div class="form-group">
              <label for="nomeClinica">Nome da Clínica *</label>
              <input
                id="nomeClinica"
                type="text"
                [(ngModel)]="nomeClinica"
                name="nomeClinica"
                placeholder="Ex: Clínica Bem Estar"
                required
                autofocus
              />
            </div>

            <div class="form-group">
              <label for="cnpj">CNPJ</label>
              <input
                id="cnpj"
                type="text"
                [(ngModel)]="cnpj"
                name="cnpj"
                placeholder="00.000.000/0001-00"
              />
            </div>

            <div class="form-row">
              <div class="form-group">
                <label for="emailClinica">Email da Clínica *</label>
                <input
                  id="emailClinica"
                  type="email"
                  [(ngModel)]="emailClinica"
                  name="emailClinica"
                  placeholder="contato@clinica.com"
                  required
                />
              </div>

              <div class="form-group">
                <label for="telefone">Telefone</label>
                <input
                  id="telefone"
                  type="tel"
                  [(ngModel)]="telefone"
                  name="telefone"
                  placeholder="(00) 00000-0000"
                />
              </div>
            </div>

            <button type="submit" class="btn-primary" [disabled]="!nomeClinica || !emailClinica">
              Próximo
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M5 12h14M12 5l7 7-7 7"/>
              </svg>
            </button>
          </form>
        }

        <!-- Step 2: Dados do Admin -->
        @if (currentStep() === 2) {
          <form class="onboarding-form" (ngSubmit)="submitOnboarding()">
            <div class="form-group">
              <label for="nomeAdmin">Seu Nome *</label>
              <input
                id="nomeAdmin"
                type="text"
                [(ngModel)]="nomeAdmin"
                name="nomeAdmin"
                placeholder="Seu nome completo"
                required
                autofocus
              />
            </div>

            <div class="form-group">
              <label for="emailAdmin">Seu Email *</label>
              <input
                id="emailAdmin"
                type="email"
                [(ngModel)]="emailAdmin"
                name="emailAdmin"
                placeholder="seu@email.com"
                required
              />
            </div>

            <div class="form-group">
              <label for="senhaAdmin">Senha *</label>
              <input
                id="senhaAdmin"
                type="password"
                [(ngModel)]="senhaAdmin"
                name="senhaAdmin"
                placeholder="Mínimo 8 caracteres (A-Z, a-z, 0-9)"
                required
                minlength="8"
              />
              <small class="form-hint">Deve conter maiúscula, minúscula e número</small>
            </div>

            <div class="form-actions">
              <button type="button" class="btn-secondary" (click)="previousStep()">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M19 12H5M12 19l-7-7 7-7"/>
                </svg>
                Voltar
              </button>
              <button
                type="submit"
                class="btn-primary"
                [disabled]="loading() || !nomeAdmin || !emailAdmin || !senhaAdmin"
              >
                @if (loading()) {
                  <span class="spinner"></span>
                  Criando...
                } @else {
                  Criar Clínica
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                    <polyline points="22 4 12 14.01 9 11.01"/>
                  </svg>
                }
              </button>
            </div>
          </form>
        }

        <!-- Step 3: Sucesso -->
        @if (currentStep() === 3) {
          <div class="onboarding-success">
            <div class="success-icon">
              <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                <polyline points="22 4 12 14.01 9 11.01"/>
              </svg>
            </div>
            <h2>Clínica criada com sucesso!</h2>
            <p>Sua clínica <strong>{{ nomeClinica }}</strong> está pronta.</p>

            <div class="checklist">
              <h3>Próximos passos:</h3>
              <label class="checklist-item">
                <input type="checkbox" checked disabled />
                <span>Criar conta e clínica</span>
              </label>
              <label class="checklist-item">
                <input type="checkbox" />
                <span>Cadastrar psicólogos</span>
              </label>
              <label class="checklist-item">
                <input type="checkbox" />
                <span>Cadastrar pacientes</span>
              </label>
              <label class="checklist-item">
                <input type="checkbox" />
                <span>Configurar plano de contas</span>
              </label>
              <label class="checklist-item">
                <input type="checkbox" />
                <span>Registrar primeiro atendimento</span>
              </label>
            </div>

            <button class="btn-primary" (click)="goToDashboard()">
              Ir para o Dashboard
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M5 12h14M12 5l7 7-7 7"/>
              </svg>
            </button>
          </div>
        }

        @if (currentStep() < 3) {
          <div class="onboarding-footer">
            <span>Já tem uma conta?</span>
            <a routerLink="/auth/login">Fazer login</a>
          </div>
        }
      </div>
    </div>
  `,
  styleUrls: ['./onboarding.component.css']
})
export class OnboardingComponent {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly authStore = inject(AuthStore);
  private readonly destroyRef = inject(DestroyRef);

  currentStep = signal(1);
  loading = signal(false);
  errorMessage = signal('');

  // Step 1
  nomeClinica = '';
  cnpj = '';
  emailClinica = '';
  telefone = '';

  // Step 2
  nomeAdmin = '';
  emailAdmin = '';
  senhaAdmin = '';

  nextStep(): void {
    this.errorMessage.set('');
    this.currentStep.update(s => s + 1);
  }

  previousStep(): void {
    this.errorMessage.set('');
    this.currentStep.update(s => s - 1);
  }

  submitOnboarding(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.api.post<OnboardingResponse>('api/onboarding', {
      nomeClinica: this.nomeClinica,
      cnpj: this.cnpj || null,
      emailClinica: this.emailClinica,
      telefone: this.telefone || null,
      nomeAdmin: this.nomeAdmin,
      emailAdmin: this.emailAdmin,
      senhaAdmin: this.senhaAdmin
    })
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
      next: (response) => {
        this.loading.set(false);
        this.authStore.loginSuccess(
          {
            id: response.usuarioId,
            nome: response.nomeUsuario,
            email: response.email,
            role: 'admin',
            clinicaId: response.clinicaId,
          },
          response.accessToken
        );
        this.currentStep.set(3);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err.error?.detail || err.error?.title || 'Erro ao criar clínica. Tente novamente.';
        this.errorMessage.set(msg);
      }
    });
  }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
