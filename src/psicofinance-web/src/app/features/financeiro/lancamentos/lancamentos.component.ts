import {
  Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../../core/services/api.service';

interface Lancamento {
  id: string;
  descricao: string;
  valor: number;
  tipo: 'Receita' | 'Despesa';
  status: 'Previsto' | 'Confirmado' | 'Cancelado';
  dataVencimento: string;
  dataPagamento: string | null;
  competencia: string;
  planoContaNome: string;
}

interface PlanoConta {
  id: string;
  nome: string;
  tipo: string;
  ativo: boolean;
}

@Component({
  selector: 'app-lancamentos',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Lançamentos Financeiros</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Controle de receitas e despesas — {{ competencia }}
        </p>
      </div>
      <button class="btn btn--primary" (click)="abrirFormulario()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
        </svg>
        Novo Lançamento
      </button>
    </div>

    <!-- Filtros -->
    <div class="card" style="margin-bottom: 16px;">
      <div class="toolbar">
        <input class="input" type="month" [(ngModel)]="competencia" (change)="carregar()" style="max-width: 180px;" />
        <select class="input" style="max-width: 150px;" [(ngModel)]="tipoFiltro" (change)="carregar()">
          <option value="">Todos os tipos</option>
          <option value="Receita">Receita</option>
          <option value="Despesa">Despesa</option>
        </select>
        <select class="input" style="max-width: 160px;" [(ngModel)]="statusFiltro" (change)="carregar()">
          <option value="">Todos os status</option>
          <option value="Previsto">Previsto</option>
          <option value="Confirmado">Confirmado</option>
          <option value="Cancelado">Cancelado</option>
        </select>
      </div>
    </div>

    <!-- Resumo -->
    <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; margin-bottom: 16px;">
      <div class="card" style="text-align: center;">
        <p class="body-text" style="color: var(--color-muted);">Receitas Previstas</p>
        <p class="heading-lg" style="color: var(--color-success);">
          {{ totalReceitas() | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
        </p>
      </div>
      <div class="card" style="text-align: center;">
        <p class="body-text" style="color: var(--color-muted);">Despesas Previstas</p>
        <p class="heading-lg" style="color: var(--color-danger);">
          {{ totalDespesas() | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
        </p>
      </div>
      <div class="card" style="text-align: center;">
        <p class="body-text" style="color: var(--color-muted);">Saldo Previsto</p>
        <p class="heading-lg" [style.color]="saldo() >= 0 ? 'var(--color-success)' : 'var(--color-danger)'">
          {{ saldo() | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
        </p>
      </div>
    </div>

    <div class="card">
      @if (loading()) {
        <div class="loading-state"><span class="spinner-md"></span></div>
      } @else if (lancamentos().length === 0) {
        <div class="empty-state"><p>Nenhum lançamento encontrado.</p></div>
      } @else {
        <table class="table">
          <thead>
            <tr>
              <th>Descrição</th>
              <th>Plano de Conta</th>
              <th>Vencimento</th>
              <th>Valor</th>
              <th>Tipo</th>
              <th>Status</th>
              <th style="width: 140px;">Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (l of lancamentos(); track l.id) {
              <tr [class.opacity-50]="l.status === 'Cancelado'">
                <td class="font-medium">{{ l.descricao }}</td>
                <td class="text-muted">{{ l.planoContaNome }}</td>
                <td>{{ l.dataVencimento | date:'dd/MM/yyyy' }}</td>
                <td [style.color]="l.tipo === 'Receita' ? 'var(--color-success)' : 'var(--color-danger)'">
                  {{ l.valor | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
                </td>
                <td>
                  <span class="badge" [class]="l.tipo === 'Receita' ? 'badge--success' : 'badge--danger'">
                    {{ l.tipo }}
                  </span>
                </td>
                <td>
                  <span class="badge" [ngClass]="{
                    'badge--neutral': l.status === 'Previsto',
                    'badge--success': l.status === 'Confirmado',
                    'badge--danger': l.status === 'Cancelado'
                  }">{{ l.status }}</span>
                </td>
                <td style="display: flex; gap: 4px;">
                  @if (l.status === 'Previsto') {
                    <button class="btn btn--ghost btn--sm" (click)="confirmar(l)">Confirmar</button>
                    <button class="btn btn--ghost btn--sm" style="color: var(--color-danger);" (click)="cancelar(l)">Cancelar</button>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>

    <!-- Modal novo lançamento -->
    @if (modalAberto()) {
      <div class="modal-overlay" (click)="fecharModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>Novo Lançamento</h3>
            <button class="btn btn--ghost btn--icon" (click)="fecharModal()">✕</button>
          </div>
          <div class="modal-body">
            @if (erro()) { <div class="alert alert--danger">{{ erro() }}</div> }
            <div class="form-group">
              <label class="form-label">Descrição *</label>
              <input class="input" [(ngModel)]="form.descricao" placeholder="Ex: Sessão paciente João" />
            </div>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px;">
              <div class="form-group">
                <label class="form-label">Valor *</label>
                <input class="input" type="number" min="0.01" step="0.01" [(ngModel)]="form.valor" />
              </div>
              <div class="form-group">
                <label class="form-label">Tipo *</label>
                <select class="input" [(ngModel)]="form.tipo">
                  <option value="Receita">Receita</option>
                  <option value="Despesa">Despesa</option>
                </select>
              </div>
            </div>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px;">
              <div class="form-group">
                <label class="form-label">Vencimento *</label>
                <input class="input" type="date" [(ngModel)]="form.dataVencimento" />
              </div>
              <div class="form-group">
                <label class="form-label">Competência *</label>
                <input class="input" type="month" [(ngModel)]="form.competencia" />
              </div>
            </div>
            <div class="form-group">
              <label class="form-label">Plano de Conta *</label>
              <select class="input" [(ngModel)]="form.planoContaId">
                <option value="">Selecione...</option>
                @for (p of planosConta(); track p.id) {
                  <option [value]="p.id">{{ p.nome }} ({{ p.tipo }})</option>
                }
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Observação</label>
              <input class="input" [(ngModel)]="form.observacao" placeholder="Opcional" />
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn--ghost" (click)="fecharModal()">Cancelar</button>
            <button class="btn btn--primary" [disabled]="salvando()" (click)="salvar()">
              {{ salvando() ? 'Salvando...' : 'Salvar' }}
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class LancamentosComponent implements OnInit {
  private api = inject(ApiService);
  private destroyRef = inject(DestroyRef);

  lancamentos = signal<Lancamento[]>([]);
  planosConta = signal<PlanoConta[]>([]);
  loading = signal(true);
  modalAberto = signal(false);
  salvando = signal(false);
  erro = signal<string | null>(null);

  tipoFiltro = '';
  statusFiltro = '';
  competencia = new Date().toISOString().slice(0, 7);

  form = { descricao: '', valor: 0, tipo: 'Receita', dataVencimento: '', competencia: this.competencia, planoContaId: '', observacao: '' };

  totalReceitas = signal(0);
  totalDespesas = signal(0);
  saldo = signal(0);

  ngOnInit() {
    this.carregarPlanosConta();
    this.carregar();
  }

  carregar() {
    this.loading.set(true);
    const params: Record<string, string> = { competencia: this.competencia };
    if (this.tipoFiltro) params['tipo'] = this.tipoFiltro;
    if (this.statusFiltro) params['status'] = this.statusFiltro;

    this.api.get<Lancamento[]>('lancamentos', params)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.lancamentos.set(data);
          this.calcularTotais(data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }

  calcularTotais(lancamentos: Lancamento[]) {
    const ativos = lancamentos.filter(l => l.status !== 'Cancelado');
    const receitas = ativos.filter(l => l.tipo === 'Receita').reduce((s, l) => s + l.valor, 0);
    const despesas = ativos.filter(l => l.tipo === 'Despesa').reduce((s, l) => s + l.valor, 0);
    this.totalReceitas.set(receitas);
    this.totalDespesas.set(despesas);
    this.saldo.set(receitas - despesas);
  }

  carregarPlanosConta() {
    this.api.get<PlanoConta[]>('planos-conta', { ativo: 'true' })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(data => this.planosConta.set(data));
  }

  abrirFormulario() {
    this.form = { descricao: '', valor: 0, tipo: 'Receita', dataVencimento: '', competencia: this.competencia, planoContaId: '', observacao: '' };
    this.erro.set(null);
    this.modalAberto.set(true);
  }

  fecharModal() { this.modalAberto.set(false); }

  salvar() {
    this.salvando.set(true);
    this.erro.set(null);

    this.api.post<Lancamento>('lancamentos', {
      descricao: this.form.descricao,
      valor: this.form.valor,
      tipo: this.form.tipo,
      dataVencimento: this.form.dataVencimento,
      competencia: this.form.competencia,
      planoContaId: this.form.planoContaId,
      observacao: this.form.observacao || null
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.salvando.set(false); this.fecharModal(); this.carregar(); },
      error: (e) => { this.salvando.set(false); this.erro.set(e?.error?.detail ?? 'Erro ao salvar.'); }
    });
  }

  confirmar(lancamento: Lancamento) {
    const data = prompt('Data de pagamento (YYYY-MM-DD):') || new Date().toISOString().slice(0, 10);
    this.api.patch(`lancamentos/${lancamento.id}/confirmar`, { dataPagamento: data })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.carregar());
  }

  cancelar(lancamento: Lancamento) {
    if (!confirm('Confirma o cancelamento deste lançamento?')) return;
    this.api.patch(`lancamentos/${lancamento.id}/cancelar`, { motivo: null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.carregar());
  }
}
