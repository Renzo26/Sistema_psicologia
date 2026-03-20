import { Component, ChangeDetectionStrategy, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

interface KpiCard {
  label: string;
  value: number;
  prefix: string;
  suffix: string;
  change: number;
  changeLabel: string;
  type: 'currency' | 'percent' | 'number';
}

interface BarData {
  month: string;
  receita: number;
  despesa: number;
  future: boolean;
}

interface RecentSession {
  paciente: string;
  psicologo: string;
  data: string;
  valor: number;
  status: 'realizada' | 'agendada' | 'faltou' | 'cancelada';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- TOP ROW: 3 KPI Cards -->
    <div class="dash-top">
      @for (kpi of kpis(); track kpi.label; let i = $index) {
        <div class="card card--compact animate-fade-up animate-delay-{{ i + 1 }}">
          <span class="label-text kpi-label">{{ kpi.label }}</span>
          @if (kpi.type === 'currency') {
            <div class="kpi-value">
              <span class="display-lg">R$ {{ formatNumber(kpi.value) }}</span>
              <span class="display-lg cents">,{{ getCents(kpi.value) }}</span>
            </div>
          } @else if (kpi.type === 'percent') {
            <div class="kpi-value">
              <span class="display-lg">{{ kpi.value }}</span>
              <span class="display-lg cents">%</span>
            </div>
          } @else {
            <div class="kpi-value">
              <span class="display-lg">{{ kpi.value }}</span>
            </div>
          }
          <div class="kpi-change" [class.kpi-change--positive]="kpi.change >= 0" [class.kpi-change--negative]="kpi.change < 0">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
              @if (kpi.change >= 0) {
                <polyline points="18 15 12 9 6 15"/>
              } @else {
                <polyline points="6 9 12 15 18 9"/>
              }
            </svg>
            <span class="label-text">{{ kpi.change > 0 ? '+' : '' }}{{ kpi.change }}%</span>
            <span class="body-text kpi-change-label">{{ kpi.changeLabel }}</span>
          </div>
        </div>
      }
    </div>

    <!-- MIDDLE ROW: Chart + Remaining -->
    <div class="dash-mid">
      <!-- Money Flow Chart -->
      <div class="card animate-fade-up animate-delay-4">
        <div class="chart-header">
          <span class="heading-md">Fluxo de Caixa</span>
          <div class="chart-legend">
            <span class="legend-item"><span class="legend-dot legend-dot--receita"></span> Receita</span>
            <span class="legend-item"><span class="legend-dot legend-dot--despesa"></span> Despesa</span>
          </div>
          <select class="input chart-period" style="width: auto; height: 30px; font-size: 12px;">
            <option>Mensal</option>
            <option>Semanal</option>
          </select>
        </div>

        <div class="chart-area">
          @for (bar of barData(); track bar.month; let i = $index) {
            <div class="chart-col" [class.chart-col--future]="bar.future">
              <div class="chart-bars">
                <div class="chart-bar chart-bar--receita animate-bar animate-bar-delay-{{ i + 1 }}"
                     [style.height.%]="getBarHeight(bar.receita, maxBar())"
                     [attr.aria-label]="'Receita ' + bar.month + ': R$ ' + bar.receita">
                </div>
                <div class="chart-bar chart-bar--despesa animate-bar animate-bar-delay-{{ i + 1 }}"
                     [style.height.%]="getBarHeight(bar.despesa, maxBar())">
                </div>
              </div>
              <span class="chart-label caption-text">{{ bar.month }}</span>
            </div>
          }
        </div>
      </div>

      <!-- Remaining / Budget Card -->
      <div class="card animate-fade-up animate-delay-5">
        <div class="remaining-header">
          <span class="heading-md">Meta Mensal</span>
        </div>
        <div class="remaining-value">
          <span class="display-xl">69</span>
          <span class="display-xl cents">%</span>
        </div>
        <p class="body-text remaining-desc" style="color: var(--color-muted);">
          Faturamento atingiu 69% da meta mensal de R$ 30.000
        </p>
        <div class="remaining-bars">
          <div class="remaining-item">
            <div class="remaining-row">
              <span class="heading-sm">Sessões</span>
              <span class="label-text" style="color: var(--color-sage-400);">89%</span>
            </div>
            <div class="progress"><div class="progress__fill" style="width: 89%"></div></div>
          </div>
          <div class="remaining-item">
            <div class="remaining-row">
              <span class="heading-sm">Recebimentos</span>
              <span class="label-text" style="color: var(--color-sage-400);">78%</span>
            </div>
            <div class="progress"><div class="progress__fill" style="width: 78%"></div></div>
          </div>
          <div class="remaining-item">
            <div class="remaining-row">
              <span class="heading-sm">Inadimplência</span>
              <span class="label-text" style="color: var(--color-warning);">42%</span>
            </div>
            <div class="progress"><div class="progress__fill" style="width: 42%; background: var(--color-warning);"></div></div>
          </div>
        </div>
      </div>
    </div>

    <!-- BOTTOM ROW: Recent Sessions Table -->
    <div class="dash-bottom">
      <div class="card animate-fade-up animate-delay-6">
        <div class="table-header">
          <span class="heading-md">Sessões Recentes</span>
          <button class="btn btn--secondary btn--sm">Ver todas</button>
        </div>
        <table class="table">
          <thead>
            <tr>
              <th class="caption-text">Paciente</th>
              <th class="caption-text">Psicólogo(a)</th>
              <th class="caption-text">Data</th>
              <th class="caption-text">Valor</th>
              <th class="caption-text">Status</th>
            </tr>
          </thead>
          <tbody>
            @for (s of sessions(); track s.paciente + s.data) {
              <tr>
                <td class="heading-sm">{{ s.paciente }}</td>
                <td class="body-text">{{ s.psicologo }}</td>
                <td class="mono-text">{{ s.data }}</td>
                <td class="heading-sm">R$ {{ s.valor.toFixed(2) }}</td>
                <td>
                  <span class="pill pill--{{ s.status }}">
                    <span class="pill__dot"></span>
                    {{ statusLabel(s.status) }}
                  </span>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }

    // ---------- TOP ROW ----------
    .dash-top {
      display: grid;
      grid-template-columns: 1fr 1fr 1fr;
      gap: 14px;
      margin-bottom: 14px;
    }

    .kpi-label {
      color: var(--color-muted);
    }

    .kpi-value {
      margin-top: 8px;
      color: var(--color-text);
    }

    .kpi-change {
      display: flex;
      align-items: center;
      gap: 4px;
      margin-top: 10px;

      &--positive {
        color: var(--color-sage-400);
      }
      &--negative {
        color: var(--color-danger);
      }
    }

    .kpi-change-label {
      color: var(--color-muted);
      margin-left: 4px;
    }

    // ---------- MIDDLE ROW ----------
    .dash-mid {
      display: grid;
      grid-template-columns: 1fr 300px;
      gap: 14px;
      margin-bottom: 14px;
    }

    .chart-header {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-bottom: 16px;

      .heading-md { flex: 1; color: var(--color-text); }
    }

    .chart-legend {
      display: flex;
      gap: 14px;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 5px;
      font-size: 11px;
      color: var(--color-muted);
    }

    .legend-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;

      &--receita { background: var(--color-primary-300); }
      &--despesa { background: var(--color-primary-100); }
    }

    .chart-period {
      padding: 4px 28px 4px 8px;
    }

    // Chart bars
    .chart-area {
      display: flex;
      align-items: flex-end;
      gap: 6px;
      height: 200px;
      padding-top: 10px;
    }

    .chart-col {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 6px;
      height: 100%;

      &--future { opacity: 0.35; }
    }

    .chart-bars {
      flex: 1;
      display: flex;
      align-items: flex-end;
      gap: 3px;
      width: 100%;
    }

    .chart-bar {
      flex: 1;
      border-radius: 4px 4px 0 0;
      min-height: 4px;
      transform-origin: bottom;

      &--receita { background: var(--color-primary-300); }
      &--despesa { background: var(--color-primary-100); }
    }

    .chart-label {
      color: var(--color-hint);
    }

    // ---------- REMAINING / BUDGET ----------
    .remaining-header {
      margin-bottom: 8px;
      .heading-md { color: var(--color-text); }
    }

    .remaining-value {
      color: var(--color-text);
      margin-bottom: 8px;
    }

    .remaining-desc {
      margin-bottom: 20px;
    }

    .remaining-bars {
      display: flex;
      flex-direction: column;
      gap: 14px;
    }

    .remaining-item {
      display: flex;
      flex-direction: column;
      gap: 5px;
    }

    .remaining-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    // ---------- BOTTOM ROW ----------
    .dash-bottom {
      margin-bottom: 24px;
    }

    .table-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 14px;

      .heading-md { color: var(--color-text); }
    }

    .table {
      width: 100%;
      border-collapse: collapse;

      th {
        text-align: left;
        padding: 8px 10px;
        border-bottom: 1px solid var(--color-border);
        color: var(--color-hint);
      }

      td {
        padding: 10px;
        border-bottom: 1px solid var(--color-border);
        color: var(--color-text);
      }

      tbody tr {
        transition: background 0.12s;

        &:hover {
          background: var(--color-surface-2);
        }
      }
    }

    // ---------- RESPONSIVE ----------
    @media (max-width: 1280px) {
      .dash-top {
        grid-template-columns: 1fr 1fr;
      }
      .dash-mid {
        grid-template-columns: 1fr;
      }
    }

    @media (max-width: 768px) {
      .dash-top {
        grid-template-columns: 1fr;
      }
      .table {
        font-size: 12px;
      }
    }
  `],
})
export class DashboardComponent implements OnInit {
  readonly kpis = signal<KpiCard[]>([
    { label: 'Faturamento Realizado', value: 18240.00, prefix: 'R$ ', suffix: '', change: 12.4, changeLabel: 'vs mês anterior', type: 'currency' },
    { label: 'Sessões Realizadas', value: 142, prefix: '', suffix: '', change: 8, changeLabel: 'vs mês anterior', type: 'number' },
    { label: 'Taxa de Inadimplência', value: 7.2, prefix: '', suffix: '%', change: -2.1, changeLabel: 'vs mês anterior', type: 'percent' },
  ]);

  readonly barData = signal<BarData[]>([
    { month: 'Jan', receita: 14200, despesa: 8400, future: false },
    { month: 'Fev', receita: 15800, despesa: 9200, future: false },
    { month: 'Mar', receita: 13600, despesa: 7800, future: false },
    { month: 'Abr', receita: 17200, despesa: 10400, future: false },
    { month: 'Mai', receita: 16800, despesa: 9600, future: false },
    { month: 'Jun', receita: 19200, despesa: 11200, future: false },
    { month: 'Jul', receita: 18240, despesa: 10800, future: false },
    { month: 'Ago', receita: 15000, despesa: 9000, future: true },
    { month: 'Set', receita: 16000, despesa: 9500, future: true },
    { month: 'Out', receita: 17000, despesa: 10000, future: true },
    { month: 'Nov', receita: 18000, despesa: 10500, future: true },
    { month: 'Dez', receita: 20000, despesa: 12000, future: true },
  ]);

  readonly maxBar = signal(0);

  readonly sessions = signal<RecentSession[]>([
    { paciente: 'Ana Clara Silva', psicologo: 'Dra. Mariana Costa', data: '20/03/2026', valor: 180.00, status: 'realizada' },
    { paciente: 'Bruno Oliveira', psicologo: 'Dr. Lucas Ferreira', data: '20/03/2026', valor: 200.00, status: 'agendada' },
    { paciente: 'Carla Mendes', psicologo: 'Dra. Mariana Costa', data: '19/03/2026', valor: 180.00, status: 'realizada' },
    { paciente: 'Diego Santos', psicologo: 'Dra. Juliana Lima', data: '19/03/2026', valor: 150.00, status: 'faltou' },
    { paciente: 'Elena Rodrigues', psicologo: 'Dr. Lucas Ferreira', data: '18/03/2026', valor: 200.00, status: 'cancelada' },
  ]);

  ngOnInit(): void {
    const bars = this.barData();
    const max = Math.max(...bars.map((b) => Math.max(b.receita, b.despesa)));
    this.maxBar.set(max);
  }

  formatNumber(value: number): string {
    const intPart = Math.floor(value);
    return intPart.toLocaleString('pt-BR');
  }

  getCents(value: number): string {
    const cents = Math.round((value % 1) * 100);
    return cents.toString().padStart(2, '0');
  }

  getBarHeight(value: number, max: number): number {
    if (max === 0) return 0;
    return (value / max) * 100;
  }

  statusLabel(status: string): string {
    const labels: Record<string, string> = {
      realizada: 'Realizada',
      agendada: 'Agendada',
      faltou: 'Faltou',
      cancelada: 'Cancelada',
    };
    return labels[status] || status;
  }
}
