import { Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../core/services/api.service';

interface SessaoDto {
  id: string;
  contratoId: string;
  pacienteNome: string;
  psicologoNome: string;
  data: string;
  horarioInicio: string;
  duracaoMinutos: number;
  status: string;
  observacoes: string | null;
  motivoFalta: string | null;
}

interface ContratoResumo { id: string; pacienteNome: string; psicologoNome: string; status: string; }
interface SessaoForm {
  contratoId: string;
  data: string;
  horarioInicio: string;
  duracaoMinutos: number;
  observacoes: string | null;
}

@Component({
  selector: 'app-sessao-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <h2 class="heading-lg">{{ editando() ? 'Editar' : 'Nova' }} Sessão</h2>
      @if (editando() && sessaoStatus() === 'Agendada') {
        <div style="display:flex;gap:8px;">
          <button class="btn btn--success" (click)="marcarPresenca()" [disabled]="loading()">✓ Presença</button>
          <button class="btn btn--warning" (click)="registrarFalta(false)" [disabled]="loading()">✕ Falta</button>
          <button class="btn btn--ghost" (click)="registrarFalta(true)" [disabled]="loading()">⚠ Justificada</button>
          <button class="btn btn--danger" (click)="confirmarCancelamento()" [disabled]="loading()">Cancelar</button>
        </div>
      }
    </div>

    @if (sessaoStatus() && sessaoStatus() !== 'Agendada') {
      <div class="alert alert--info animate-fade-up">
        Status atual: <strong>{{ traduzirStatus(sessaoStatus()) }}</strong>
        @if (motivoFalta()) { — {{ motivoFalta() }} }
      </div>
    }
    @if (successMsg()) { <div class="alert alert--success animate-fade-up">{{ successMsg() }}</div> }
    @if (errorMsg()) { <div class="alert alert--error animate-fade-up">{{ errorMsg() }}</div> }

    @if (showCancelar()) {
      <div class="card" style="border:1px solid #fecaca; margin-bottom:16px;">
        <h3 class="heading-md" style="color:#dc2626; margin-bottom:12px;">Cancelar Sessão</h3>
        <div class="form-group">
          <label class="label-text">Motivo (opcional)</label>
          <textarea class="input" rows="2" [(ngModel)]="motivoCancelamento"
                    placeholder="Motivo do cancelamento..."></textarea>
        </div>
        <div style="display:flex; gap:8px; justify-content:flex-end; margin-top:12px;">
          <button class="btn btn--secondary" (click)="showCancelar.set(false)">Voltar</button>
          <button class="btn btn--danger" (click)="cancelar()" [disabled]="loading()">Confirmar</button>
        </div>
      </div>
    }

    <form class="config-form" (ngSubmit)="salvar()">
      <div class="card">
        <h3 class="heading-md section-title">Contrato</h3>
        <div class="form-group">
          <label class="label-text">Contrato (Paciente / Psicólogo) *</label>
          <select class="input" [(ngModel)]="form.contratoId" name="contratoId" required
                  [disabled]="editando()">
            <option value="">Selecione o contrato...</option>
            @for (c of contratos(); track c.id) {
              <option [value]="c.id">{{ c.pacienteNome }} → {{ c.psicologoNome }}</option>
            }
          </select>
        </div>
      </div>

      <div class="card">
        <h3 class="heading-md section-title">Data e Horário</h3>
        <div class="form-grid">
          <div class="form-group">
            <label class="label-text">Data *</label>
            <input class="input" type="date" [(ngModel)]="form.data" name="data" required
                   [disabled]="sessaoStatus() !== '' && sessaoStatus() !== 'Agendada'" />
          </div>
          <div class="form-group">
            <label class="label-text">Horário *</label>
            <input class="input" type="time" [(ngModel)]="form.horarioInicio" name="horarioInicio" required
                   [disabled]="sessaoStatus() !== '' && sessaoStatus() !== 'Agendada'" />
          </div>
          <div class="form-group">
            <label class="label-text">Duração (min) *</label>
            <input class="input" type="number" min="15" max="240" [(ngModel)]="form.duracaoMinutos"
                   name="duracaoMinutos" required
                   [disabled]="sessaoStatus() !== '' && sessaoStatus() !== 'Agendada'" />
          </div>
        </div>
      </div>

      <div class="card">
        <h3 class="heading-md section-title">Observações</h3>
        <div class="form-group">
          <textarea class="input" rows="3" [(ngModel)]="form.observacoes" name="observacoes"
                    placeholder="Observações sobre a sessão..."
                    [disabled]="sessaoStatus() !== '' && sessaoStatus() !== 'Agendada'"></textarea>
        </div>
      </div>

      <div class="form-actions">
        <button type="button" class="btn btn--secondary" (click)="voltar()">Cancelar</button>
        @if (!editando() || sessaoStatus() === 'Agendada') {
          <button type="submit" class="btn btn--primary" [disabled]="loading() || !form.contratoId || !form.data">
            @if (loading()) { <span class="spinner"></span> Salvando... } @else { Salvar }
          </button>
        }
      </div>
    </form>
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; flex-wrap: wrap; gap: 8px; }
    .config-form { display: flex; flex-direction: column; gap: 16px; }
    .section-title { color: var(--color-text); margin-bottom: 16px; padding-bottom: 12px; border-bottom: 1px solid var(--color-border); }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 14px; }
    .form-group { display: flex; flex-direction: column; gap: 4px; }
    .form-actions { display: flex; justify-content: flex-end; gap: 10px; padding-top: 8px; }
    .alert { padding: 10px 14px; border-radius: var(--radius-lg); font-size: 13px; margin-bottom: 16px; }
    .alert--success { background: #f0fdf4; color: #196040; border: 1px solid #bbf7d0; }
    .alert--error { background: #fef2f2; color: #dc2626; border: 1px solid #fecaca; }
    .alert--info { background: var(--color-surface-2); color: var(--color-muted); border: 1px solid var(--color-border); }
    .btn--success { background:#16a34a; color:#fff; border:none; padding:7px 14px; border-radius:var(--radius-lg); cursor:pointer; font-size:13px; font-weight:500; }
    .btn--success:hover { background:#15803d; }
    .btn--warning { background:#d97706; color:#fff; border:none; padding:7px 14px; border-radius:var(--radius-lg); cursor:pointer; font-size:13px; font-weight:500; }
    .btn--warning:hover { background:#b45309; }
    .btn--danger { background:#dc2626; color:#fff; border:none; padding:7px 14px; border-radius:var(--radius-lg); cursor:pointer; font-size:13px; font-weight:500; }
    .btn--danger:hover { background:#b91c1c; }
    .btn--success:disabled, .btn--warning:disabled, .btn--danger:disabled { opacity:.5; cursor:not-allowed; }
    .spinner { width:14px; height:14px; border:2px solid rgba(255,255,255,.3); border-top-color:#fff; border-radius:50%; animation:spin .6s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    @media (max-width: 768px) { .form-grid { grid-template-columns: 1fr; } }
  `],
})
export class SessaoFormComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  loading = signal(false);
  editando = signal(false);
  successMsg = signal('');
  errorMsg = signal('');
  sessaoStatus = signal('');
  motivoFalta = signal<string | null>(null);
  showCancelar = signal(false);
  contratos = signal<ContratoResumo[]>([]);
  motivoCancelamento = '';
  private id: string | null = null;

  form: SessaoForm = {
    contratoId: '',
    data: '',
    horarioInicio: '08:00',
    duracaoMinutos: 50,
    observacoes: null,
  };

  ngOnInit(): void {
    this.carregarContratos();
    this.id = this.route.snapshot.paramMap.get('id');
    if (this.id) {
      this.editando.set(true);
      this.carregarSessao(this.id);
    } else {
      this.form.data = new Date().toISOString().split('T')[0];
    }
  }

  private carregarContratos(): void {
    this.api.get<ContratoResumo[]>('contratos?status=Ativo')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (data) => this.contratos.set(data) });
  }

  private carregarSessao(id: string): void {
    this.loading.set(true);
    this.api.get<SessaoDto>(`sessoes/${id}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.sessaoStatus.set(data.status);
          this.motivoFalta.set(data.motivoFalta);
          this.form = {
            contratoId: data.contratoId,
            data: data.data,
            horarioInicio: data.horarioInicio.substring(0, 5),
            duracaoMinutos: data.duracaoMinutos,
            observacoes: data.observacoes,
          };
          this.loading.set(false);
        },
        error: () => { this.loading.set(false); this.errorMsg.set('Erro ao carregar sessão.'); },
      });
  }

  salvar(): void {
    this.loading.set(true);
    this.successMsg.set(''); this.errorMsg.set('');

    const req$ = this.editando()
      ? this.api.put<SessaoDto>(`sessoes/${this.id}`, {
          data: this.form.data,
          horarioInicio: this.form.horarioInicio + ':00',
          duracaoMinutos: this.form.duracaoMinutos,
          observacoes: this.form.observacoes,
        })
      : this.api.post<SessaoDto>('sessoes', {
          contratoId: this.form.contratoId,
          data: this.form.data,
          horarioInicio: this.form.horarioInicio + ':00',
          duracaoMinutos: this.form.duracaoMinutos,
          observacoes: this.form.observacoes,
        });

    req$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.loading.set(false);
        this.successMsg.set(this.editando() ? 'Sessão atualizada!' : 'Sessão agendada!');
        if (!this.editando()) { this.id = result.id; this.editando.set(true); this.sessaoStatus.set('Agendada'); }
        setTimeout(() => this.successMsg.set(''), 3000);
      },
      error: (err) => { this.loading.set(false); this.errorMsg.set(err.error?.detail || 'Erro ao salvar sessão.'); },
    });
  }

  marcarPresenca(): void {
    if (!this.id) return;
    this.loading.set(true);
    this.api.patch<void>(`sessoes/${this.id}/presenca`, {})
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => { this.loading.set(false); this.sessaoStatus.set('Realizada'); this.successMsg.set('Presença marcada!'); setTimeout(() => this.successMsg.set(''), 3000); },
        error: (err) => { this.loading.set(false); this.errorMsg.set(err.error?.detail || 'Erro.'); },
      });
  }

  registrarFalta(justificada: boolean): void {
    if (!this.id) return;
    this.loading.set(true);
    this.api.patch<void>(`sessoes/${this.id}/falta`, { justificada, motivo: null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => { this.loading.set(false); this.sessaoStatus.set(justificada ? 'FaltaJustificada' : 'Falta'); this.successMsg.set('Falta registrada!'); setTimeout(() => this.successMsg.set(''), 3000); },
        error: (err) => { this.loading.set(false); this.errorMsg.set(err.error?.detail || 'Erro.'); },
      });
  }

  confirmarCancelamento(): void { this.showCancelar.set(true); }

  cancelar(): void {
    if (!this.id) return;
    this.loading.set(true);
    this.api.patch<void>(`sessoes/${this.id}/cancelar`, { motivo: this.motivoCancelamento || null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => { this.loading.set(false); this.sessaoStatus.set('Cancelada'); this.showCancelar.set(false); this.successMsg.set('Sessão cancelada.'); setTimeout(() => this.successMsg.set(''), 3000); },
        error: (err) => { this.loading.set(false); this.errorMsg.set(err.error?.detail || 'Erro ao cancelar.'); },
      });
  }

  traduzirStatus(status: string): string {
    const map: Record<string, string> = { 'Agendada': 'Agendada', 'Realizada': 'Realizada', 'Falta': 'Falta', 'FaltaJustificada': 'Falta Justificada', 'Cancelada': 'Cancelada' };
    return map[status] ?? status;
  }

  voltar(): void { this.router.navigate(['/sessoes']); }
}
