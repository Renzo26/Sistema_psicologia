import { Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../../core/services/api.service';

interface ClinicaDto {
  id: string;
  nome: string;
  cnpj: string | null;
  email: string;
  telefone: string | null;
  cep: string | null;
  logradouro: string | null;
  numero: string | null;
  complemento: string | null;
  bairro: string | null;
  cidade: string | null;
  estado: string | null;
  horarioEnvioAlerta: string;
  webhookN8nUrl: string | null;
  timezone: string;
  ativo: boolean;
  criadoEm: string;
}

@Component({
  selector: 'app-clinica-config',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <h2 class="heading-lg">Configurações da Clínica</h2>
      <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
        Gerencie os dados da sua clínica
      </p>
    </div>

    @if (successMessage()) {
      <div class="alert alert--success animate-fade-up">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>
        </svg>
        <span class="body-text">{{ successMessage() }}</span>
      </div>
    }

    @if (errorMessage()) {
      <div class="alert alert--error animate-fade-up">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/>
          <line x1="9" y1="9" x2="15" y2="15"/>
        </svg>
        <span class="body-text">{{ errorMessage() }}</span>
      </div>
    }

    <form class="config-form" (ngSubmit)="salvar()">
      <!-- Dados Básicos -->
      <div class="card animate-fade-up">
        <h3 class="heading-md section-title">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
            <polyline points="9 22 9 12 15 12 15 22"/>
          </svg>
          Dados da Clínica
        </h3>

        <div class="form-grid">
          <div class="form-group form-group--full">
            <label class="label-text" for="nome">Nome da Clínica *</label>
            <input class="input" id="nome" type="text" [(ngModel)]="form.nome" name="nome" required />
          </div>

          <div class="form-group">
            <label class="label-text" for="cnpj">CNPJ</label>
            <input class="input" id="cnpj" type="text" [(ngModel)]="form.cnpj" name="cnpj"
                   placeholder="00.000.000/0001-00" />
          </div>

          <div class="form-group">
            <label class="label-text" for="email">Email *</label>
            <input class="input" id="email" type="email" [(ngModel)]="form.email" name="email" required />
          </div>

          <div class="form-group">
            <label class="label-text" for="telefone">Telefone</label>
            <input class="input" id="telefone" type="tel" [(ngModel)]="form.telefone" name="telefone"
                   placeholder="(00) 00000-0000" />
          </div>
        </div>
      </div>

      <!-- Endereço -->
      <div class="card animate-fade-up">
        <h3 class="heading-md section-title">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/>
            <circle cx="12" cy="10" r="3"/>
          </svg>
          Endereço
        </h3>

        <div class="form-grid">
          <div class="form-group">
            <label class="label-text" for="cep">CEP</label>
            <input class="input" id="cep" type="text" [(ngModel)]="form.cep" name="cep"
                   placeholder="00000-000" />
          </div>

          <div class="form-group form-group--2">
            <label class="label-text" for="logradouro">Logradouro</label>
            <input class="input" id="logradouro" type="text" [(ngModel)]="form.logradouro" name="logradouro" />
          </div>

          <div class="form-group form-group--sm">
            <label class="label-text" for="numero">Número</label>
            <input class="input" id="numero" type="text" [(ngModel)]="form.numero" name="numero" />
          </div>

          <div class="form-group">
            <label class="label-text" for="complemento">Complemento</label>
            <input class="input" id="complemento" type="text" [(ngModel)]="form.complemento" name="complemento" />
          </div>

          <div class="form-group">
            <label class="label-text" for="bairro">Bairro</label>
            <input class="input" id="bairro" type="text" [(ngModel)]="form.bairro" name="bairro" />
          </div>

          <div class="form-group">
            <label class="label-text" for="cidade">Cidade</label>
            <input class="input" id="cidade" type="text" [(ngModel)]="form.cidade" name="cidade" />
          </div>

          <div class="form-group form-group--sm">
            <label class="label-text" for="estado">UF</label>
            <select class="input" id="estado" [(ngModel)]="form.estado" name="estado">
              <option [ngValue]="null">--</option>
              @for (uf of ufs; track uf) {
                <option [value]="uf">{{ uf }}</option>
              }
            </select>
          </div>
        </div>
      </div>

      <!-- Configurações Avançadas -->
      <div class="card animate-fade-up">
        <h3 class="heading-md section-title">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="3"/>
            <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09a1.65 1.65 0 0 0-1-1.51 1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/>
          </svg>
          Configurações
        </h3>

        <div class="form-grid">
          <div class="form-group">
            <label class="label-text" for="horario">Horário de Envio de Alertas</label>
            <input class="input" id="horario" type="time" [(ngModel)]="form.horarioEnvioAlerta" name="horarioEnvioAlerta" />
          </div>

          <div class="form-group">
            <label class="label-text" for="timezone">Timezone</label>
            <select class="input" id="timezone" [(ngModel)]="form.timezone" name="timezone">
              <option value="America/Sao_Paulo">America/Sao_Paulo (BRT)</option>
              <option value="America/Manaus">America/Manaus (AMT)</option>
              <option value="America/Belem">America/Belem (BRT)</option>
              <option value="America/Fortaleza">America/Fortaleza (BRT)</option>
              <option value="America/Recife">America/Recife (BRT)</option>
              <option value="America/Cuiaba">America/Cuiaba (AMT)</option>
              <option value="America/Porto_Velho">America/Porto_Velho (AMT)</option>
              <option value="America/Rio_Branco">America/Rio_Branco (ACT)</option>
            </select>
          </div>

          <div class="form-group form-group--full">
            <label class="label-text" for="webhook">Webhook N8N (URL)</label>
            <input class="input" id="webhook" type="url" [(ngModel)]="form.webhookN8nUrl" name="webhookN8nUrl"
                   placeholder="https://n8n.seudominio.com/webhook/..." />
            <span class="caption-text" style="color: var(--color-hint); margin-top: 2px;">
              URL do webhook N8N para automações e notificações
            </span>
          </div>
        </div>
      </div>

      <!-- Ações -->
      <div class="form-actions">
        <button type="button" class="btn btn--secondary" (click)="carregar()" [disabled]="loading()">
          Cancelar
        </button>
        <button type="submit" class="btn btn--primary" [disabled]="loading() || !form.nome || !form.email">
          @if (loading()) {
            <span class="spinner"></span>
            Salvando...
          } @else {
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/>
              <polyline points="17 21 17 13 7 13 7 21"/><polyline points="7 3 7 8 15 8"/>
            </svg>
            Salvar Alterações
          }
        </button>
      </div>
    </form>
  `,
  styles: [`
    :host { display: block; }

    .page-header { margin-bottom: 20px; }
    .page-header h2 { color: var(--color-text); }

    .alert {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 10px 14px;
      border-radius: var(--radius-lg);
      font-size: 13px;
      margin-bottom: 16px;
    }

    .alert--success {
      background: var(--color-success-bg, #f0fdf4);
      color: #196040;
      border: 1px solid #bbf7d0;
    }

    .alert--error {
      background: #fef2f2;
      color: #dc2626;
      border: 1px solid #fecaca;
    }

    .config-form {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .section-title {
      display: flex;
      align-items: center;
      gap: 8px;
      color: var(--color-text);
      margin-bottom: 16px;
      padding-bottom: 12px;
      border-bottom: 1px solid var(--color-border);
    }

    .section-title svg { color: var(--color-primary-300); flex-shrink: 0; }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr 1fr;
      gap: 14px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .form-group label { color: var(--color-muted); }

    .form-group--full { grid-column: 1 / -1; }
    .form-group--2 { grid-column: span 2; }
    .form-group--sm { max-width: 120px; }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 10px;
      padding-top: 8px;
    }

    .spinner {
      width: 14px;
      height: 14px;
      border: 2px solid rgba(255,255,255,.3);
      border-top-color: #fff;
      border-radius: 50%;
      animation: spin .6s linear infinite;
    }

    @keyframes spin { to { transform: rotate(360deg); } }

    @media (max-width: 768px) {
      .form-grid { grid-template-columns: 1fr; }
      .form-group--2 { grid-column: span 1; }
      .form-group--sm { max-width: none; }
    }
  `],
})
export class ClinicaConfigComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  loading = signal(false);
  successMessage = signal('');
  errorMessage = signal('');

  form: ClinicaForm = this.criarFormVazio();

  readonly ufs = [
    'AC','AL','AP','AM','BA','CE','DF','ES','GO','MA','MT','MS',
    'MG','PA','PB','PR','PE','PI','RJ','RN','RS','RO','RR','SC',
    'SP','SE','TO'
  ];

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.loading.set(true);
    this.limparMensagens();

    this.api.get<ClinicaDto>('clinicas/minha')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.form = {
            nome: data.nome,
            cnpj: data.cnpj,
            email: data.email,
            telefone: data.telefone,
            cep: data.cep,
            logradouro: data.logradouro,
            numero: data.numero,
            complemento: data.complemento,
            bairro: data.bairro,
            cidade: data.cidade,
            estado: data.estado,
            horarioEnvioAlerta: data.horarioEnvioAlerta ?? '08:00',
            webhookN8nUrl: data.webhookN8nUrl,
            timezone: data.timezone ?? 'America/Sao_Paulo',
          };
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Erro ao carregar dados da clínica.');
        },
      });
  }

  salvar(): void {
    this.loading.set(true);
    this.limparMensagens();

    this.api.put<ClinicaDto>('clinicas/minha', this.form)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.successMessage.set('Dados salvos com sucesso!');
          setTimeout(() => this.successMessage.set(''), 4000);
        },
        error: (err) => {
          this.loading.set(false);
          const msg = err.error?.detail || err.error?.title || 'Erro ao salvar. Tente novamente.';
          this.errorMessage.set(msg);
        },
      });
  }

  private limparMensagens(): void {
    this.successMessage.set('');
    this.errorMessage.set('');
  }

  private criarFormVazio(): ClinicaForm {
    return {
      nome: '', cnpj: null, email: '', telefone: null,
      cep: null, logradouro: null, numero: null, complemento: null,
      bairro: null, cidade: null, estado: null,
      horarioEnvioAlerta: '08:00', webhookN8nUrl: null, timezone: 'America/Sao_Paulo',
    };
  }
}

interface ClinicaForm {
  nome: string;
  cnpj: string | null;
  email: string;
  telefone: string | null;
  cep: string | null;
  logradouro: string | null;
  numero: string | null;
  complemento: string | null;
  bairro: string | null;
  cidade: string | null;
  estado: string | null;
  horarioEnvioAlerta: string;
  webhookN8nUrl: string | null;
  timezone: string;
}
