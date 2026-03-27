import { Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';

interface ContratoDto {
  id: string;
  pacienteId: string;
  pacienteNome: string;
  psicologoId: string;
  psicologoNome: string;
  valorSessao: number;
  formaPagamento: string;
  frequencia: string;
  diaSemanaSessao: string;
  horarioSessao: string;
  duracaoMinutos: number;
  cobraFaltaInjustificada: boolean;
  cobraFaltaJustificada: boolean;
  dataInicio: string;
  dataFim: string | null;
  status: string;
  motivoEncerramento: string | null;
  planoContaId: string | null;
  observacoes: string | null;
}

interface ContratoForm {
  pacienteId: string;
  psicologoId: string;
  valorSessao: number;
  formaPagamento: string;
  frequencia: string;
  diaSemanaSessao: string;
  horarioSessao: string;
  duracaoMinutos: number;
  cobraFaltaInjustificada: boolean;
  cobraFaltaJustificada: boolean;
  dataInicio: string;
  dataFim: string | null;
  planoContaId: string | null;
  observacoes: string | null;
}

interface PacienteResumo { id: string; nome: string; }
interface PsicologoResumo { id: string; nome: string; }

@Component({
  selector: 'app-contrato-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <h2 class="heading-lg">{{ editando() ? 'Editar' : 'Novo' }} Contrato</h2>
      @if (editando() && contratoStatus() !== 'Encerrado') {
        <button class="btn btn--danger" (click)="confirmarEncerramento()" [disabled]="loading()">
          Encerrar Contrato
        </button>
      }
    </div>

    @if (contratoStatus() === 'Encerrado') {
      <div class="alert alert--warning animate-fade-up">
        Este contrato está encerrado e não pode ser editado.
      </div>
    }

    @if (successMsg()) {
      <div class="alert alert--success animate-fade-up">{{ successMsg() }}</div>
    }
    @if (errorMsg()) {
      <div class="alert alert--error animate-fade-up">{{ errorMsg() }}</div>
    }

    @if (showEncerrar()) {
      <div class="card" style="border: 1px solid #fecaca; margin-bottom: 16px;">
        <h3 class="heading-md section-title" style="color: #dc2626;">Encerrar Contrato</h3>
        <div class="form-group">
          <label class="label-text">Motivo do encerramento</label>
          <textarea class="input" rows="3" [(ngModel)]="motivoEncerramento"
                    placeholder="Informe o motivo do encerramento..."></textarea>
        </div>
        <div class="form-actions" style="padding-top: 12px;">
          <button class="btn btn--secondary" (click)="showEncerrar.set(false)">Cancelar</button>
          <button class="btn btn--danger" (click)="encerrar()" [disabled]="loading()">
            Confirmar Encerramento
          </button>
        </div>
      </div>
    }

    <form class="config-form" (ngSubmit)="salvar()">
      <div class="card">
        <h3 class="heading-md section-title">Paciente e Psicólogo</h3>
        <div class="form-grid">
          <div class="form-group">
            <label class="label-text">Paciente *</label>
            <select class="input" [(ngModel)]="form.pacienteId" name="pacienteId" required
                    [disabled]="contratoStatus() === 'Encerrado'">
              <option value="">Selecione...</option>
              @for (p of pacientes(); track p.id) {
                <option [value]="p.id">{{ p.nome }}</option>
              }
            </select>
          </div>
          <div class="form-group">
            <label class="label-text">Psicólogo *</label>
            <select class="input" [(ngModel)]="form.psicologoId" name="psicologoId" required
                    [disabled]="contratoStatus() === 'Encerrado'">
              <option value="">Selecione...</option>
              @for (p of psicologos(); track p.id) {
                <option [value]="p.id">{{ p.nome }}</option>
              }
            </select>
          </div>
        </div>
      </div>

      <div class="card">
        <h3 class="heading-md section-title">Configuração da Sessão</h3>
        <div class="form-grid">
          <div class="form-group">
            <label class="label-text">Valor da Sessão (R$) *</label>
            <input class="input" type="number" step="0.01" min="0" [(ngModel)]="form.valorSessao" name="valorSessao" required
                   [disabled]="contratoStatus() === 'Encerrado'" />
          </div>
          <div class="form-group">
            <label class="label-text">Forma de Pagamento *</label>
            <select class="input" [(ngModel)]="form.formaPagamento" name="formaPagamento" required
                    [disabled]="contratoStatus() === 'Encerrado'">
              <option value="Pix">PIX</option>
              <option value="CartaoCredito">Cartão Crédito</option>
              <option value="CartaoDebito">Cartão Débito</option>
              <option value="Dinheiro">Dinheiro</option>
              <option value="Convenio">Convênio</option>
              <option value="Transferencia">Transferência</option>
            </select>
          </div>
          <div class="form-group">
            <label class="label-text">Frequência *</label>
            <select class="input" [(ngModel)]="form.frequencia" name="frequencia" required
                    [disabled]="contratoStatus() === 'Encerrado'">
              <option value="Semanal">Semanal</option>
              <option value="Quinzenal">Quinzenal</option>
            </select>
          </div>
          <div class="form-group">
            <label class="label-text">Dia da Semana *</label>
            <select class="input" [(ngModel)]="form.diaSemanaSessao" name="diaSemanaSessao" required
                    [disabled]="contratoStatus() === 'Encerrado'">
              <option value="Segunda">Segunda</option>
              <option value="Terca">Terça</option>
              <option value="Quarta">Quarta</option>
              <option value="Quinta">Quinta</option>
              <option value="Sexta">Sexta</option>
              <option value="Sabado">Sábado</option>
              <option value="Domingo">Domingo</option>
            </select>
          </div>
          <div class="form-group">
            <label class="label-text">Horário *</label>
            <input class="input" type="time" [(ngModel)]="form.horarioSessao" name="horarioSessao" required
                   [disabled]="contratoStatus() === 'Encerrado'" />
          </div>
          <div class="form-group">
            <label class="label-text">Duração (min) *</label>
            <input class="input" type="number" min="15" max="240" [(ngModel)]="form.duracaoMinutos" name="duracaoMinutos" required
                   [disabled]="contratoStatus() === 'Encerrado'" />
          </div>
        </div>
      </div>

      <div class="card">
        <h3 class="heading-md section-title">Vigência e Regras</h3>
        <div class="form-grid">
          <div class="form-group">
            <label class="label-text">Data de Início *</label>
            <input class="input" type="date" [(ngModel)]="form.dataInicio" name="dataInicio" required
                   [disabled]="contratoStatus() === 'Encerrado'" />
          </div>
          <div class="form-group">
            <label class="label-text">Data de Término</label>
            <input class="input" type="date" [(ngModel)]="form.dataFim" name="dataFim"
                   [disabled]="contratoStatus() === 'Encerrado'" />
          </div>
          <div class="form-group">
            <label class="label-text">Plano de Conta</label>
            <select class="input" [(ngModel)]="form.planoContaId" name="planoContaId"
                    [disabled]="contratoStatus() === 'Encerrado'">
              <option [ngValue]="null">Nenhum</option>
            </select>
          </div>
          <div class="form-group form-group--full">
            <div class="checkbox-group">
              <label class="toggle-label">
                <input type="checkbox" [(ngModel)]="form.cobraFaltaInjustificada" name="cobraFaltaInjustificada"
                       [disabled]="contratoStatus() === 'Encerrado'" />
                <span class="body-text">Cobrar falta injustificada</span>
              </label>
              <label class="toggle-label">
                <input type="checkbox" [(ngModel)]="form.cobraFaltaJustificada" name="cobraFaltaJustificada"
                       [disabled]="contratoStatus() === 'Encerrado'" />
                <span class="body-text">Cobrar falta justificada</span>
              </label>
            </div>
          </div>
        </div>
      </div>

      <div class="card">
        <h3 class="heading-md section-title">Observações</h3>
        <div class="form-group">
          <textarea class="input" rows="4" [(ngModel)]="form.observacoes" name="observacoes"
                    placeholder="Observações sobre o contrato..."
                    [disabled]="contratoStatus() === 'Encerrado'"></textarea>
        </div>
      </div>

      <div class="form-actions">
        <button type="button" class="btn btn--secondary" (click)="voltar()">Cancelar</button>
        @if (contratoStatus() !== 'Encerrado') {
          <button type="submit" class="btn btn--primary" [disabled]="loading() || !form.pacienteId || !form.psicologoId">
            @if (loading()) { <span class="spinner"></span> Salvando... } @else { Salvar }
          </button>
        }
      </div>
    </form>
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .config-form { display: flex; flex-direction: column; gap: 16px; }
    .section-title { color: var(--color-text); margin-bottom: 16px; padding-bottom: 12px; border-bottom: 1px solid var(--color-border); }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 14px; }
    .form-group { display: flex; flex-direction: column; gap: 4px; }
    .form-group label { color: var(--color-muted); }
    .form-group--full { grid-column: 1 / -1; }
    .form-group--2 { grid-column: span 2; }
    .form-actions { display: flex; justify-content: flex-end; gap: 10px; padding-top: 8px; }
    .checkbox-group { display: flex; gap: 24px; }
    .toggle-label { display: flex; align-items: center; gap: 6px; cursor: pointer; }
    .toggle-label input { accent-color: var(--color-primary-300); }
    .alert { padding: 10px 14px; border-radius: var(--radius-lg); font-size: 13px; margin-bottom: 16px; }
    .alert--success { background: var(--color-success-bg, #f0fdf4); color: #196040; border: 1px solid #bbf7d0; }
    .alert--error { background: #fef2f2; color: #dc2626; border: 1px solid #fecaca; }
    .alert--warning { background: #fffbeb; color: #92400e; border: 1px solid #fde68a; }
    .btn--danger { background: #dc2626; color: #fff; border: none; padding: 8px 16px; border-radius: var(--radius-lg); cursor: pointer; font-size: 13px; font-weight: 500; }
    .btn--danger:hover { background: #b91c1c; }
    .btn--danger:disabled { opacity: .5; cursor: not-allowed; }
    .spinner { width: 14px; height: 14px; border: 2px solid rgba(255,255,255,.3); border-top-color: #fff; border-radius: 50%; animation: spin .6s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    @media (max-width: 768px) { .form-grid { grid-template-columns: 1fr; } .form-group--2 { grid-column: span 1; } .checkbox-group { flex-direction: column; } }
  `],
})
export class ContratoFormComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  loading = signal(false);
  editando = signal(false);
  successMsg = signal('');
  errorMsg = signal('');
  contratoStatus = signal('');
  showEncerrar = signal(false);
  pacientes = signal<PacienteResumo[]>([]);
  psicologos = signal<PsicologoResumo[]>([]);
  motivoEncerramento = '';
  private id: string | null = null;

  form: ContratoForm = {
    pacienteId: '',
    psicologoId: '',
    valorSessao: 0,
    formaPagamento: 'Pix',
    frequencia: 'Semanal',
    diaSemanaSessao: 'Segunda',
    horarioSessao: '08:00',
    duracaoMinutos: 50,
    cobraFaltaInjustificada: true,
    cobraFaltaJustificada: false,
    dataInicio: '',
    dataFim: null,
    planoContaId: null,
    observacoes: null,
  };

  ngOnInit(): void {
    this.carregarCombos();
    this.id = this.route.snapshot.paramMap.get('id');
    if (this.id) {
      this.editando.set(true);
      this.carregarContrato(this.id);
    }
  }

  private carregarCombos(): void {
    forkJoin({
      pacientes: this.api.get<PacienteResumo[]>('pacientes?apenasAtivos=true'),
      psicologos: this.api.get<PsicologoResumo[]>('psicologos?apenasAtivos=true'),
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: ({ pacientes, psicologos }) => {
        this.pacientes.set(pacientes);
        this.psicologos.set(psicologos);
      },
    });
  }

  private carregarContrato(id: string): void {
    this.loading.set(true);
    this.api.get<ContratoDto>(`contratos/${id}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.contratoStatus.set(data.status);
          this.form = {
            pacienteId: data.pacienteId,
            psicologoId: data.psicologoId,
            valorSessao: data.valorSessao,
            formaPagamento: data.formaPagamento,
            frequencia: data.frequencia,
            diaSemanaSessao: data.diaSemanaSessao,
            horarioSessao: data.horarioSessao.substring(0, 5),
            duracaoMinutos: data.duracaoMinutos,
            cobraFaltaInjustificada: data.cobraFaltaInjustificada,
            cobraFaltaJustificada: data.cobraFaltaJustificada,
            dataInicio: data.dataInicio,
            dataFim: data.dataFim,
            planoContaId: data.planoContaId,
            observacoes: data.observacoes,
          };
          this.loading.set(false);
        },
        error: () => { this.loading.set(false); this.errorMsg.set('Erro ao carregar contrato.'); },
      });
  }

  salvar(): void {
    this.loading.set(true);
    this.successMsg.set(''); this.errorMsg.set('');

    const req$ = this.editando()
      ? this.api.put<ContratoDto>(`contratos/${this.id}`, this.form)
      : this.api.post<ContratoDto>('contratos', this.form);

    req$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.loading.set(false);
        this.successMsg.set(this.editando() ? 'Contrato atualizado!' : 'Contrato cadastrado!');
        if (!this.editando()) {
          this.id = result.id;
          this.editando.set(true);
        }
        setTimeout(() => this.successMsg.set(''), 3000);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.detail || err.error?.title || 'Erro ao salvar contrato.');
      },
    });
  }

  confirmarEncerramento(): void {
    this.showEncerrar.set(true);
  }

  encerrar(): void {
    if (!this.id) return;
    this.loading.set(true);
    this.successMsg.set(''); this.errorMsg.set('');

    this.api.patch<void>(`contratos/${this.id}/encerrar`, { motivoEncerramento: this.motivoEncerramento })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.contratoStatus.set('Encerrado');
          this.showEncerrar.set(false);
          this.successMsg.set('Contrato encerrado com sucesso!');
          setTimeout(() => this.successMsg.set(''), 3000);
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMsg.set(err.error?.detail || err.error?.title || 'Erro ao encerrar contrato.');
        },
      });
  }

  voltar(): void { this.router.navigate(['/cadastros/contratos']); }
}
