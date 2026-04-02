import {
  Component, ChangeDetectionStrategy, inject, signal, computed, OnInit, DestroyRef
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  DocumentosService,
  ReciboDto,
  RecibosFilter,
  EmitirReciboCommand
} from '../documentos.service';

@Component({
  selector: 'app-recibos-listagem',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Recibos</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Emissão e gestão de recibos de sessões
        </p>
      </div>
      <button class="btn btn--primary" (click)="abrirEmitirModal()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
        </svg>
        Emitir Recibo
      </button>
    </div>

    <!-- Filtros -->
    <div class="card" style="margin-bottom: 16px;">
      <div class="toolbar">
        <input
          class="input"
          type="text"
          placeholder="Filtrar por paciente..."
          [(ngModel)]="filtroPacienteNome"
          style="max-width: 240px;"
        />
        <input class="input" type="date" [(ngModel)]="filtroDataInicio" (change)="carregar()" style="max-width: 160px;" />
        <span class="body-text" style="color: var(--color-muted); align-self: center;">até</span>
        <input class="input" type="date" [(ngModel)]="filtroDataFim" (change)="carregar()" style="max-width: 160px;" />
        <select class="input" style="max-width: 160px;" [(ngModel)]="filtroStatus" (change)="carregar()">
          <option value="">Todos os status</option>
          <option value="Gerado">Gerado</option>
          <option value="Cancelado">Cancelado</option>
        </select>
        <button class="btn btn--ghost btn--sm" (click)="limparFiltros()">Limpar</button>
      </div>
    </div>

    <!-- Tabela -->
    <div class="card">
      @if (loading()) {
        <div class="loading-state"><span class="spinner-md"></span></div>
      } @else if (recibosFiltrados().length === 0) {
        <div class="empty-state">
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="var(--color-hint)" stroke-width="1.5">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
            <polyline points="14 2 14 8 20 8"/>
          </svg>
          <p style="margin-top: 12px;">Nenhum recibo encontrado.</p>
        </div>
      } @else {
        <table class="table">
          <thead>
            <tr>
              <th>Número</th>
              <th>Paciente</th>
              <th>Psicólogo</th>
              <th>Valor</th>
              <th>Data Emissão</th>
              <th>Status</th>
              <th style="width: 160px;">Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (recibo of recibosFiltrados(); track recibo.id) {
              <tr [class.opacity-50]="recibo.status === 'Cancelado'">
                <td class="font-medium">{{ recibo.numeroRecibo }}</td>
                <td>{{ recibo.pacienteNome }}</td>
                <td class="text-muted">{{ recibo.psicologoNome }}</td>
                <td style="font-weight: 600;">
                  {{ recibo.valor | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
                </td>
                <td>{{ recibo.dataEmissao | date:'dd/MM/yyyy' }}</td>
                <td>
                  <span class="badge" [class]="recibo.status === 'Gerado' ? 'badge--success' : 'badge--danger'">
                    {{ recibo.status }}
                  </span>
                </td>
                <td style="display: flex; gap: 4px;">
                  @if (recibo.arquivoUrl) {
                    <button
                      class="btn btn--ghost btn--sm"
                      (click)="baixarPdf(recibo)"
                      title="Baixar PDF"
                    >
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
                        <polyline points="7 10 12 15 17 10"/>
                        <line x1="12" y1="15" x2="12" y2="3"/>
                      </svg>
                      PDF
                    </button>
                  }
                  @if (recibo.status === 'Gerado') {
                    <button
                      class="btn btn--ghost btn--sm"
                      style="color: var(--color-danger);"
                      (click)="cancelar(recibo)"
                    >Cancelar</button>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>

    <!-- Modal emitir recibo -->
    @if (modalEmitirAberto()) {
      <div class="modal-overlay" (click)="fecharEmitirModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>Emitir Recibo</h3>
            <button class="btn btn--ghost btn--icon" (click)="fecharEmitirModal()">✕</button>
          </div>
          <div class="modal-body">
            @if (erro()) {
              <div class="alert alert--danger">{{ erro() }}</div>
            }
            <div class="form-group">
              <label class="form-label">ID da Sessão *</label>
              <input
                class="input"
                [(ngModel)]="emitirForm.sessaoId"
                placeholder="Ex: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
              />
              <span class="body-text" style="color: var(--color-muted); font-size: 12px; margin-top: 4px; display: block;">
                Informe o identificador único da sessão para emissão do recibo.
              </span>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn--ghost" (click)="fecharEmitirModal()">Cancelar</button>
            <button
              class="btn btn--primary"
              [disabled]="salvando() || !emitirForm.sessaoId.trim()"
              (click)="emitirRecibo()"
            >
              {{ salvando() ? 'Emitindo...' : 'Emitir Recibo' }}
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class RecibosListagemComponent implements OnInit {
  private service = inject(DocumentosService);
  private destroyRef = inject(DestroyRef);

  recibos = signal<ReciboDto[]>([]);
  loading = signal(true);
  modalEmitirAberto = signal(false);
  salvando = signal(false);
  erro = signal<string | null>(null);

  filtroPacienteNome = '';
  filtroDataInicio = '';
  filtroDataFim = '';
  filtroStatus = '';

  emitirForm: EmitirReciboCommand = { sessaoId: '' };

  recibosFiltrados = computed(() => {
    const termo = this.filtroPacienteNome.toLowerCase().trim();
    if (!termo) return this.recibos();
    return this.recibos().filter(r =>
      r.pacienteNome.toLowerCase().includes(termo)
    );
  });

  ngOnInit() { this.carregar(); }

  carregar() {
    this.loading.set(true);
    const filter: RecibosFilter = {};
    if (this.filtroDataInicio) filter.dataInicio = this.filtroDataInicio;
    if (this.filtroDataFim) filter.dataFim = this.filtroDataFim;
    if (this.filtroStatus) filter.status = this.filtroStatus;

    this.service.getRecibos(filter)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.recibos.set(data); this.loading.set(false); },
        error: () => this.loading.set(false)
      });
  }

  limparFiltros() {
    this.filtroPacienteNome = '';
    this.filtroDataInicio = '';
    this.filtroDataFim = '';
    this.filtroStatus = '';
    this.carregar();
  }

  abrirEmitirModal() {
    this.emitirForm = { sessaoId: '' };
    this.erro.set(null);
    this.modalEmitirAberto.set(true);
  }

  fecharEmitirModal() { this.modalEmitirAberto.set(false); }

  emitirRecibo() {
    if (!this.emitirForm.sessaoId.trim()) return;
    this.salvando.set(true);
    this.erro.set(null);

    this.service.emitirRecibo(this.emitirForm)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.salvando.set(false);
          this.fecharEmitirModal();
          this.carregar();
        },
        error: (e: { error?: { detail?: string } }) => {
          this.salvando.set(false);
          this.erro.set(e?.error?.detail ?? 'Erro ao emitir recibo.');
        }
      });
  }

  baixarPdf(recibo: ReciboDto) {
    this.service.downloadReciboPdf(recibo.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `recibo-${recibo.numeroRecibo}.pdf`;
          link.click();
          URL.revokeObjectURL(url);
        },
        error: () => alert('Erro ao baixar o PDF do recibo.')
      });
  }

  cancelar(recibo: ReciboDto) {
    if (!confirm(`Confirma o cancelamento do recibo ${recibo.numeroRecibo}?`)) return;
    this.service.cancelarRecibo(recibo.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.carregar());
  }
}
