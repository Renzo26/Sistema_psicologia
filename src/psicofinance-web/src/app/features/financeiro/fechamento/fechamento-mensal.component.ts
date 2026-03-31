import {
  Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../../core/services/api.service';

interface FechamentoResumo {
  id: string;
  mesReferencia: string;
  status: 'Aberto' | 'Fechado';
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
  totalSessoesRealizadas: number;
  fechadoEm: string | null;
}

interface FechamentoDetalhe extends FechamentoResumo {
  totalSessoes: number;
  totalSessoesFalta: number;
  observacao: string | null;
  repassesPorPsicologo: RepasseConsolidado[];
}

interface RepasseConsolidado {
  psicologoId: string;
  psicologoNome: string;
  totalSessoes: number;
  totalReceitas: number;
  valorRepasse: number;
}

@Component({
  selector: 'app-fechamento-mensal',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Fechamento Mensal</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Consolidação financeira por período
        </p>
      </div>
      <button class="btn btn-primary" (click)="abrirNovoFechamento()">
        + Realizar fechamento
      </button>
    </div>

    <!-- Modal de novo fechamento -->
    @if (mostrarModal()) {
      <div class="modal-backdrop" (click)="fecharModal()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <h3 class="heading-sm" style="margin-bottom:16px;">Realizar Fechamento Mensal</h3>
          <div class="form-group">
            <label class="form-label">Mês de referência</label>
            <input type="month" class="form-control"
                   [(ngModel)]="novoFechamento.mesReferencia" />
          </div>
          <div class="form-group">
            <label class="form-label">Observação (opcional)</label>
            <textarea class="form-control" rows="3"
                      [(ngModel)]="novoFechamento.observacao"></textarea>
          </div>
          @if (erroModal()) {
            <div class="alert alert-error" style="margin-bottom:12px;">{{ erroModal() }}</div>
          }
          <div style="display:flex;gap:8px;justify-content:flex-end;margin-top:16px;">
            <button class="btn btn-secondary" (click)="fecharModal()">Cancelar</button>
            <button class="btn btn-primary" (click)="realizarFechamento()" [disabled]="salvando()">
              {{ salvando() ? 'Salvando...' : 'Confirmar fechamento' }}
            </button>
          </div>
        </div>
      </div>
    }

    <!-- Lista de fechamentos -->
    @if (loading()) {
      <div class="loading-spinner">Carregando...</div>
    } @else {
      <div class="card">
        <div class="card-header" style="display:flex;align-items:center;gap:12px;">
          <h3 class="heading-sm">Histórico de fechamentos</h3>
          <select class="form-control" style="width:160px" [(ngModel)]="filtroStatus" (change)="carregar()">
            <option value="">Todos</option>
            <option value="Aberto">Aberto</option>
            <option value="Fechado">Fechado</option>
          </select>
        </div>
        @if (fechamentos().length === 0) {
          <p class="empty-state">Nenhum fechamento encontrado.</p>
        } @else {
          <div class="table-container">
            <table class="table">
              <thead>
                <tr>
                  <th>Mês</th>
                  <th>Status</th>
                  <th class="text-right">Receitas</th>
                  <th class="text-right">Despesas</th>
                  <th class="text-right">Saldo</th>
                  <th class="text-right">Sessões realizadas</th>
                  <th>Fechado em</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                @for (f of fechamentos(); track f.id) {
                  <tr>
                    <td><strong>{{ f.mesReferencia }}</strong></td>
                    <td>
                      <span class="badge" [class.badge-success]="f.status === 'Fechado'" [class.badge-warning]="f.status === 'Aberto'">
                        {{ f.status }}
                      </span>
                    </td>
                    <td class="text-right text-success">{{ f.totalReceitas | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
                    <td class="text-right text-danger">{{ f.totalDespesas | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
                    <td class="text-right" [style.color]="f.saldo >= 0 ? 'var(--color-success)' : 'var(--color-danger)'">
                      {{ f.saldo | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
                    </td>
                    <td class="text-right">{{ f.totalSessoesRealizadas }}</td>
                    <td>{{ f.fechadoEm ? (f.fechadoEm | date:'dd/MM/yyyy HH:mm') : '—' }}</td>
                    <td>
                      <button class="btn btn-sm btn-secondary" (click)="verDetalhe(f.mesReferencia)">
                        Detalhes
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>

      <!-- Detalhe do fechamento selecionado -->
      @if (detalhe(); as d) {
        <div class="card" style="margin-top:24px;">
          <div class="card-header" style="display:flex;justify-content:space-between;align-items:center;">
            <h3 class="heading-sm">Detalhe — {{ d.mesReferencia }}</h3>
            <button class="btn btn-sm btn-secondary" (click)="detalhe.set(null)">Fechar</button>
          </div>

          <div class="grid grid-3" style="margin:16px 0;">
            <div>
              <span class="label-sm">Total sessões</span>
              <p class="body-text">{{ d.totalSessoes }}</p>
            </div>
            <div>
              <span class="label-sm">Sessões realizadas</span>
              <p class="body-text">{{ d.totalSessoesRealizadas }}</p>
            </div>
            <div>
              <span class="label-sm">Faltas</span>
              <p class="body-text">{{ d.totalSessoesFalta }}</p>
            </div>
          </div>

          @if (d.observacao) {
            <p class="body-text" style="margin-bottom:16px;color:var(--color-muted);">
              <strong>Obs:</strong> {{ d.observacao }}
            </p>
          }

          <h4 class="heading-sm" style="margin-bottom:12px;">Repasses por psicólogo</h4>
          @if (d.repassesPorPsicologo.length === 0) {
            <p class="body-text" style="color:var(--color-muted);">Nenhum repasse consolidado.</p>
          } @else {
            <table class="table">
              <thead>
                <tr>
                  <th>Psicólogo</th>
                  <th class="text-right">Sessões</th>
                  <th class="text-right">Receitas</th>
                  <th class="text-right">Repasse</th>
                </tr>
              </thead>
              <tbody>
                @for (r of d.repassesPorPsicologo; track r.psicologoId) {
                  <tr>
                    <td>{{ r.psicologoNome }}</td>
                    <td class="text-right">{{ r.totalSessoes }}</td>
                    <td class="text-right">{{ r.totalReceitas | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
                    <td class="text-right text-warning">{{ r.valorRepasse | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>
      }
    }
  `,
})
export class FechamentoMensalComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly fechamentos = signal<FechamentoResumo[]>([]);
  readonly detalhe = signal<FechamentoDetalhe | null>(null);
  readonly loading = signal(false);
  readonly mostrarModal = signal(false);
  readonly salvando = signal(false);
  readonly erroModal = signal<string | null>(null);

  filtroStatus = '';
  novoFechamento = { mesReferencia: this.mesAtual(), observacao: '' };

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.loading.set(true);
    const params: Record<string, string> = {};
    if (this.filtroStatus) params['status'] = this.filtroStatus;

    this.api.get<FechamentoResumo[]>('fechamentos', params)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.fechamentos.set(data); this.loading.set(false); },
        error: () => this.loading.set(false),
      });
  }

  verDetalhe(mes: string): void {
    this.api.get<FechamentoDetalhe>(`fechamentos/${mes}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (d) => this.detalhe.set(d) });
  }

  abrirNovoFechamento(): void {
    this.novoFechamento = { mesReferencia: this.mesAtual(), observacao: '' };
    this.erroModal.set(null);
    this.mostrarModal.set(true);
  }

  fecharModal(): void {
    this.mostrarModal.set(false);
  }

  realizarFechamento(): void {
    if (!this.novoFechamento.mesReferencia) {
      this.erroModal.set('Informe o mês de referência.');
      return;
    }
    this.salvando.set(true);
    this.erroModal.set(null);

    this.api.post<FechamentoDetalhe>('fechamentos', {
      mesReferencia: this.novoFechamento.mesReferencia,
      observacao: this.novoFechamento.observacao || null,
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.salvando.set(false);
          this.mostrarModal.set(false);
          this.carregar();
        },
        error: (err) => {
          this.salvando.set(false);
          this.erroModal.set(err?.error?.detail ?? 'Erro ao realizar fechamento.');
        },
      });
  }

  private mesAtual(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }
}
