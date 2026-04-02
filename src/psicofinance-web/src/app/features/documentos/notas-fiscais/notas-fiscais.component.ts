import {
  Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  DocumentosService,
  NotaFiscalDto,
  NotasFiscaisFilter,
  EmitirNotaFiscalCommand
} from '../documentos.service';

type NotaFiscalStatus = NotaFiscalDto['status'];

@Component({
  selector: 'app-notas-fiscais',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Notas Fiscais (NFS-e)</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Emissão e gestão de notas fiscais de serviço
        </p>
      </div>
      <button class="btn btn--primary" (click)="abrirEmitirModal()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
        </svg>
        Emitir Nota Fiscal
      </button>
    </div>

    <!-- Aviso integração pendente -->
    <div class="alert alert--neutral" style="margin-bottom: 16px; display: flex; align-items: flex-start; gap: 10px;">
      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" style="flex-shrink: 0; margin-top: 1px;">
        <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
      </svg>
      <span class="body-text">
        <strong>Integração com a prefeitura em desenvolvimento.</strong>
        As notas fiscais são registradas no sistema e enviadas para a fila de emissão.
        A comunicação direta com a API da prefeitura está pendente de homologação.
      </span>
    </div>

    <!-- Filtros -->
    <div class="card" style="margin-bottom: 16px;">
      <div class="toolbar">
        <input
          class="input"
          type="text"
          placeholder="ID do Paciente..."
          [(ngModel)]="filtroPacienteId"
          (change)="carregar()"
          style="max-width: 300px;"
        />
        <input class="input" type="month" [(ngModel)]="filtroCompetenciaInicio" (change)="carregar()" style="max-width: 160px;" />
        <span class="body-text" style="color: var(--color-muted); align-self: center;">até</span>
        <input class="input" type="month" [(ngModel)]="filtroCompetenciaFim" (change)="carregar()" style="max-width: 160px;" />
        <select class="input" style="max-width: 160px;" [(ngModel)]="filtroStatus" (change)="carregar()">
          <option value="">Todos os status</option>
          <option value="Pendente">Pendente</option>
          <option value="Emitida">Emitida</option>
          <option value="Cancelada">Cancelada</option>
          <option value="Erro">Erro</option>
        </select>
        <button class="btn btn--ghost btn--sm" (click)="limparFiltros()">Limpar</button>
      </div>
    </div>

    <!-- Tabela -->
    <div class="card">
      @if (loading()) {
        <div class="loading-state"><span class="spinner-md"></span></div>
      } @else if (notasFiscais().length === 0) {
        <div class="empty-state">
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="var(--color-hint)" stroke-width="1.5">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
            <polyline points="14 2 14 8 20 8"/>
            <line x1="16" y1="13" x2="8" y2="13"/>
            <line x1="16" y1="17" x2="8" y2="17"/>
          </svg>
          <p style="margin-top: 12px;">Nenhuma nota fiscal encontrada.</p>
        </div>
      } @else {
        <table class="table">
          <thead>
            <tr>
              <th>Número NFS-e</th>
              <th>Paciente</th>
              <th>Competência</th>
              <th>Valor</th>
              <th>Status</th>
              <th>Data Emissão</th>
              <th style="width: 120px;">Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (nota of notasFiscais(); track nota.id) {
              <tr [class.opacity-50]="nota.status === 'Cancelada'">
                <td class="font-medium">{{ nota.numeroNfse ?? '—' }}</td>
                <td>{{ nota.pacienteNome }}</td>
                <td>{{ nota.competencia | date:'MM/yyyy' }}</td>
                <td style="font-weight: 600;">
                  {{ nota.valorServico | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
                </td>
                <td>
                  <span class="badge" [ngClass]="badgeClassNota(nota.status)">
                    {{ nota.status }}
                  </span>
                  @if (nota.status === 'Erro' && nota.erroMensagem) {
                    <span
                      class="body-text"
                      style="color: var(--color-warning); font-size: 11px; display: block; margin-top: 2px;"
                      [title]="nota.erroMensagem"
                    >
                      {{ nota.erroMensagem | slice:0:40 }}{{ nota.erroMensagem.length > 40 ? '...' : '' }}
                    </span>
                  }
                </td>
                <td>{{ nota.dataEmissao ? (nota.dataEmissao | date:'dd/MM/yyyy') : '—' }}</td>
                <td>
                  @if (nota.status !== 'Cancelada') {
                    <button
                      class="btn btn--ghost btn--sm"
                      style="color: var(--color-danger);"
                      (click)="cancelar(nota)"
                    >Cancelar</button>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>

    <!-- Modal emitir nota fiscal -->
    @if (modalEmitirAberto()) {
      <div class="modal-overlay" (click)="fecharEmitirModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>Emitir Nota Fiscal</h3>
            <button class="btn btn--ghost btn--icon" (click)="fecharEmitirModal()">✕</button>
          </div>
          <div class="modal-body">
            @if (erro()) {
              <div class="alert alert--danger">{{ erro() }}</div>
            }
            <div class="form-group">
              <label class="form-label">ID do Paciente *</label>
              <input
                class="input"
                [(ngModel)]="emitirForm.pacienteId"
                placeholder="Ex: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
              />
            </div>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px;">
              <div class="form-group">
                <label class="form-label">Valor do Serviço *</label>
                <input
                  class="input"
                  type="number"
                  min="0.01"
                  step="0.01"
                  [(ngModel)]="emitirForm.valorServico"
                />
              </div>
              <div class="form-group">
                <label class="form-label">Competência *</label>
                <input
                  class="input"
                  type="month"
                  [(ngModel)]="competenciaMes"
                />
                <span class="body-text" style="color: var(--color-muted); font-size: 12px; margin-top: 4px; display: block;">
                  Selecione o mês/ano de referência.
                </span>
              </div>
            </div>
            <div class="form-group">
              <label class="form-label">Descrição do Serviço *</label>
              <input
                class="input"
                [(ngModel)]="emitirForm.descricaoServico"
                placeholder="Ex: Serviços de Psicologia"
              />
            </div>
            <div class="form-group">
              <label class="form-label">ID do Lançamento (opcional)</label>
              <input
                class="input"
                [(ngModel)]="lancamentoIdOpcional"
                placeholder="Deixe em branco se não houver"
              />
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn--ghost" (click)="fecharEmitirModal()">Cancelar</button>
            <button
              class="btn btn--primary"
              [disabled]="salvando() || !emitirFormValido()"
              (click)="emitirNotaFiscal()"
            >
              {{ salvando() ? 'Emitindo...' : 'Emitir Nota Fiscal' }}
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class NotasFiscaisComponent implements OnInit {
  private service = inject(DocumentosService);
  private destroyRef = inject(DestroyRef);

  notasFiscais = signal<NotaFiscalDto[]>([]);
  loading = signal(true);
  modalEmitirAberto = signal(false);
  salvando = signal(false);
  erro = signal<string | null>(null);

  filtroPacienteId = '';
  filtroCompetenciaInicio = '';
  filtroCompetenciaFim = '';
  filtroStatus = '';

  emitirForm: EmitirNotaFiscalCommand = {
    pacienteId: '',
    valorServico: 0,
    descricaoServico: 'Serviços de Psicologia',
    competencia: '',
    lancamentoId: null
  };

  competenciaMes = new Date().toISOString().slice(0, 7);
  lancamentoIdOpcional = '';

  ngOnInit() { this.carregar(); }

  carregar() {
    this.loading.set(true);
    const filter: NotasFiscaisFilter = {};
    if (this.filtroPacienteId.trim()) filter.pacienteId = this.filtroPacienteId.trim();
    if (this.filtroCompetenciaInicio) filter.competenciaInicio = `${this.filtroCompetenciaInicio}-01`;
    if (this.filtroCompetenciaFim) filter.competenciaFim = `${this.filtroCompetenciaFim}-01`;
    if (this.filtroStatus) filter.status = this.filtroStatus;

    this.service.getNotasFiscais(filter)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.notasFiscais.set(data); this.loading.set(false); },
        error: () => this.loading.set(false)
      });
  }

  limparFiltros() {
    this.filtroPacienteId = '';
    this.filtroCompetenciaInicio = '';
    this.filtroCompetenciaFim = '';
    this.filtroStatus = '';
    this.carregar();
  }

  badgeClassNota(status: NotaFiscalStatus): string {
    const map: Record<NotaFiscalStatus, string> = {
      Pendente: 'badge--neutral',
      Emitida: 'badge--success',
      Cancelada: 'badge--danger',
      Erro: 'badge--warning'
    };
    return map[status];
  }

  emitirFormValido(): boolean {
    return (
      this.emitirForm.pacienteId.trim().length > 0 &&
      this.emitirForm.valorServico > 0 &&
      this.emitirForm.descricaoServico.trim().length > 0 &&
      this.competenciaMes.length > 0
    );
  }

  abrirEmitirModal() {
    this.emitirForm = {
      pacienteId: '',
      valorServico: 0,
      descricaoServico: 'Serviços de Psicologia',
      competencia: '',
      lancamentoId: null
    };
    this.competenciaMes = new Date().toISOString().slice(0, 7);
    this.lancamentoIdOpcional = '';
    this.erro.set(null);
    this.modalEmitirAberto.set(true);
  }

  fecharEmitirModal() { this.modalEmitirAberto.set(false); }

  emitirNotaFiscal() {
    if (!this.emitirFormValido()) return;
    this.salvando.set(true);
    this.erro.set(null);

    const command: EmitirNotaFiscalCommand = {
      pacienteId: this.emitirForm.pacienteId.trim(),
      valorServico: this.emitirForm.valorServico,
      descricaoServico: this.emitirForm.descricaoServico.trim(),
      competencia: `${this.competenciaMes}-01`,
      lancamentoId: this.lancamentoIdOpcional.trim() || null
    };

    this.service.emitirNotaFiscal(command)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.salvando.set(false);
          this.fecharEmitirModal();
          this.carregar();
        },
        error: (e: { error?: { detail?: string } }) => {
          this.salvando.set(false);
          this.erro.set(e?.error?.detail ?? 'Erro ao emitir nota fiscal.');
        }
      });
  }

  cancelar(nota: NotaFiscalDto) {
    const ref = nota.numeroNfse ?? nota.id.slice(0, 8);
    if (!confirm(`Confirma o cancelamento da nota fiscal ${ref}?`)) return;
    this.service.cancelarNotaFiscal(nota.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.carregar());
  }
}
