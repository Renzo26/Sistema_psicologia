import {
  Component, ChangeDetectionStrategy, inject, signal, computed, OnInit
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';

interface FluxoCaixaDia {
  data: string;
  receitasPrevisto: number;
  receitasConfirmado: number;
  despesasPrevisto: number;
  despesasConfirmado: number;
}

interface FluxoCaixa {
  competencia: string;
  totalReceitasPrevisto: number;
  totalReceitasConfirmado: number;
  totalDespesasPrevisto: number;
  totalDespesasConfirmado: number;
  saldoPrevisto: number;
  saldoRealizado: number;
  dias: FluxoCaixaDia[];
}

@Component({
  selector: 'app-fluxo-caixa',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Fluxo de Caixa</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Visão mensal de receitas e despesas — {{ competencia() }}
        </p>
      </div>
      <div style="display:flex;gap:8px;align-items:center;">
        <button class="btn btn-secondary" (click)="mesAnterior()">‹ Mês anterior</button>
        <input type="month" class="form-control" style="width:160px"
               [ngModel]="competencia()" (ngModelChange)="competencia.set($event)"
               (change)="carregar()" />
        <button class="btn btn-secondary" (click)="mesSeguinte()">Próximo mês ›</button>
      </div>
    </div>

    @if (loading()) {
      <div class="loading-spinner">Carregando...</div>
    }

    @if (fluxo(); as f) {
      <!-- Resumo -->
      <div class="grid grid-4" style="margin-bottom:24px;">
        <div class="card stat-card">
          <span class="label-sm">Receitas previstas</span>
          <strong class="heading-md text-success">{{ f.totalReceitasPrevisto | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong>
        </div>
        <div class="card stat-card">
          <span class="label-sm">Receitas realizadas</span>
          <strong class="heading-md text-success">{{ f.totalReceitasConfirmado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong>
        </div>
        <div class="card stat-card">
          <span class="label-sm">Despesas previstas</span>
          <strong class="heading-md text-danger">{{ f.totalDespesasPrevisto | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong>
        </div>
        <div class="card stat-card">
          <span class="label-sm">Despesas realizadas</span>
          <strong class="heading-md text-danger">{{ f.totalDespesasConfirmado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong>
        </div>
      </div>

      <!-- Saldo -->
      <div class="grid grid-2" style="margin-bottom:24px;">
        <div class="card stat-card" [class.positivo]="f.saldoPrevisto >= 0" [class.negativo]="f.saldoPrevisto < 0">
          <span class="label-sm">Saldo previsto</span>
          <strong class="heading-lg" [style.color]="f.saldoPrevisto >= 0 ? 'var(--color-success)' : 'var(--color-danger)'">
            {{ f.saldoPrevisto | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
          </strong>
        </div>
        <div class="card stat-card">
          <span class="label-sm">Saldo realizado</span>
          <strong class="heading-lg" [style.color]="f.saldoRealizado >= 0 ? 'var(--color-success)' : 'var(--color-danger)'">
            {{ f.saldoRealizado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
          </strong>
        </div>
      </div>

      <!-- Tabela diária -->
      <div class="card">
        <div class="card-header">
          <h3 class="heading-sm">Movimentação diária</h3>
        </div>
        @if (f.dias.length === 0) {
          <p class="empty-state">Nenhuma movimentação para este período.</p>
        } @else {
          <div class="table-container">
            <table class="table">
              <thead>
                <tr>
                  <th>Data</th>
                  <th class="text-right">Receitas (Prev.)</th>
                  <th class="text-right">Receitas (Real.)</th>
                  <th class="text-right">Despesas (Prev.)</th>
                  <th class="text-right">Despesas (Real.)</th>
                  <th class="text-right">Saldo do dia</th>
                </tr>
              </thead>
              <tbody>
                @for (dia of f.dias; track dia.data) {
                  <tr>
                    <td>{{ dia.data | date:'dd/MM/yyyy' }}</td>
                    <td class="text-right text-success">{{ dia.receitasPrevisto | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
                    <td class="text-right text-success">{{ dia.receitasConfirmado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
                    <td class="text-right text-danger">{{ dia.despesasPrevisto | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
                    <td class="text-right text-danger">{{ dia.despesasConfirmado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
                    <td class="text-right"
                        [style.color]="(dia.receitasConfirmado - dia.despesasConfirmado) >= 0 ? 'var(--color-success)' : 'var(--color-danger)'">
                      {{ (dia.receitasConfirmado - dia.despesasConfirmado) | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>
    }

    @if (erro()) {
      <div class="alert alert-error">{{ erro() }}</div>
    }
  `,
})
export class FluxoCaixaComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly competencia = signal(this.mesAtual());
  readonly fluxo = signal<FluxoCaixa | null>(null);
  readonly loading = signal(false);
  readonly erro = signal<string | null>(null);

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.loading.set(true);
    this.erro.set(null);
    this.api.get<FluxoCaixa>(`lancamentos/fluxo-caixa/${this.competencia()}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.fluxo.set(data); this.loading.set(false); },
        error: () => { this.erro.set('Erro ao carregar fluxo de caixa.'); this.loading.set(false); },
      });
  }

  mesAnterior(): void {
    const [ano, mes] = this.competencia().split('-').map(Number);
    const d = new Date(ano, mes - 2, 1);
    this.competencia.set(this.formatMes(d));
    this.carregar();
  }

  mesSeguinte(): void {
    const [ano, mes] = this.competencia().split('-').map(Number);
    const d = new Date(ano, mes, 1);
    this.competencia.set(this.formatMes(d));
    this.carregar();
  }

  private mesAtual(): string {
    return this.formatMes(new Date());
  }

  private formatMes(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }
}
