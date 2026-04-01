import {
  Component, ChangeDetectionStrategy, inject, signal, computed, OnInit, DestroyRef
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe, PercentPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../core/services/api.service';

// ── Interfaces ────────────────────────────────────────────────────
interface RankingPsicologo {
  psicologoId: string;
  nome: string;
  totalSessoes: number;
  sessoesRealizadas: number;
  receitaGerada: number;
  taxaAbsenteismo: number;
}

interface PacienteInadimplente {
  pacienteId: string;
  nome: string;
  totalLancamentosVencidos: number;
  valorTotal: number;
  vencimentoMaisAntigo: string;
}

interface KpisDashboard {
  competencia: string;
  receitaPrevista: number;
  receitaRealizada: number;
  despesaPrevista: number;
  despesaRealizada: number;
  saldoPrevisto: number;
  saldoRealizado: number;
  ticketMedio: number;
  taxaInadimplencia: number;
  totalSessoesAgendadas: number;
  totalSessoesRealizadas: number;
  totalSessoesFalta: number;
  totalSessoesFaltaJustificada: number;
  totalSessoesCanceladas: number;
  taxaAbsenteismo: number;
  taxaOcupacao: number;
  rankingPsicologos: RankingPsicologo[];
  pacientesInadimplentes: PacienteInadimplente[];
}

interface FluxoMes {
  competencia: string;
  receitasConfirmado: number;
  despesasConfirmado: number;
  receitasPrevisto: number;
  despesasPrevisto: number;
  saldoRealizado: number;
  saldoPrevisto: number;
}

type PeriodoFiltro = 'mes' | 'trimestre' | 'semestre' | 'ano';

// ── Component ─────────────────────────────────────────────────────
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe, PercentPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Dashboard</h2>
        <p class="body-text" style="color:var(--color-muted);margin-top:4px">
          Visão geral — {{ competencia() }}
        </p>
      </div>
      <div style="display:flex;gap:8px;align-items:center">
        <div class="periodo-tabs">
          @for (p of periodos; track p.value) {
            <button class="periodo-tab" [class.active]="periodo() === p.value"
                    (click)="setPeriodo(p.value)">{{ p.label }}</button>
          }
        </div>
        <input type="month" class="form-control" style="width:150px"
               [ngModel]="competencia()" (ngModelChange)="onCompetenciaChange($event)" />
        <button class="btn btn-secondary" (click)="carregar()">↺ Atualizar</button>
      </div>
    </div>

    @if (loading()) {
      <div class="loading-overlay">
        <div class="loading-spinner-lg"></div>
      </div>
    }

    @if (kpis(); as k) {
      <!-- ── KPI Cards ── -->
      <div class="kpi-grid">
        <div class="kpi-card kpi-receita">
          <span class="kpi-label">Receita realizada</span>
          <strong class="kpi-value">{{ k.receitaRealizada | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong>
          <span class="kpi-sub">Previsto: {{ k.receitaPrevista | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
          <div class="kpi-bar">
            <div class="kpi-bar-fill kpi-bar-green"
                 [style.width.%]="k.receitaPrevista > 0 ? (k.receitaRealizada / k.receitaPrevista * 100) : 0"></div>
          </div>
        </div>

        <div class="kpi-card kpi-despesa">
          <span class="kpi-label">Despesa realizada</span>
          <strong class="kpi-value">{{ k.despesaRealizada | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong>
          <span class="kpi-sub">Previsto: {{ k.despesaPrevista | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
          <div class="kpi-bar">
            <div class="kpi-bar-fill kpi-bar-red"
                 [style.width.%]="k.despesaPrevista > 0 ? (k.despesaRealizada / k.despesaPrevista * 100) : 0"></div>
          </div>
        </div>

        <div class="kpi-card" [class.kpi-saldo-pos]="k.saldoRealizado >= 0" [class.kpi-saldo-neg]="k.saldoRealizado < 0">
          <span class="kpi-label">Saldo realizado</span>
          <strong class="kpi-value" [style.color]="k.saldoRealizado >= 0 ? 'var(--color-success)' : 'var(--color-danger)'">
            {{ k.saldoRealizado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
          </strong>
          <span class="kpi-sub">Previsto: {{ k.saldoPrevisto | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
        </div>

        <div class="kpi-card">
          <span class="kpi-label">Ticket médio</span>
          <strong class="kpi-value">{{ k.ticketMedio | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</strong>
          <span class="kpi-sub">Por sessão realizada</span>
        </div>

        <div class="kpi-card">
          <span class="kpi-label">Sessões realizadas</span>
          <strong class="kpi-value">{{ k.totalSessoesRealizadas }}<small style="font-size:.65em;font-weight:400"> / {{ k.totalSessoesAgendadas }}</small></strong>
          <span class="kpi-sub">Taxa: {{ k.taxaOcupacao }}%</span>
          <div class="kpi-bar">
            <div class="kpi-bar-fill kpi-bar-blue"
                 [style.width.%]="k.totalSessoesAgendadas > 0 ? (k.totalSessoesRealizadas / k.totalSessoesAgendadas * 100) : 0"></div>
          </div>
        </div>

        <div class="kpi-card" [class.kpi-alert]="k.taxaAbsenteismo > 20">
          <span class="kpi-label">Taxa de absenteísmo</span>
          <strong class="kpi-value" [style.color]="k.taxaAbsenteismo > 20 ? 'var(--color-danger)' : 'var(--color-text)'">
            {{ k.taxaAbsenteismo }}%
          </strong>
          <span class="kpi-sub">{{ k.totalSessoesFalta + k.totalSessoesFaltaJustificada }} faltas no período</span>
        </div>

        <div class="kpi-card" [class.kpi-alert]="k.taxaInadimplencia > 10">
          <span class="kpi-label">Taxa de inadimplência</span>
          <strong class="kpi-value" [style.color]="k.taxaInadimplencia > 10 ? 'var(--color-danger)' : 'var(--color-text)'">
            {{ k.taxaInadimplencia }}%
          </strong>
          <span class="kpi-sub">{{ k.pacientesInadimplentes.length }} pacientes em atraso</span>
        </div>
      </div>

      <!-- ── Linha 2: Gráficos ── -->
      <div class="grid-2-1" style="margin:24px 0;gap:20px;display:grid;grid-template-columns:2fr 1fr">

        <!-- Fluxo de Caixa (multi-mês) -->
        <div class="card">
          <div class="card-header">
            <h3 class="heading-sm">Fluxo de Caixa — últimos 6 meses</h3>
          </div>
          @if (fluxo().length > 0) {
            <div class="chart-container" style="height:220px;padding:16px 0">
              <div class="bar-chart">
                @for (m of fluxo(); track m.competencia) {
                  <div class="bar-group">
                    <div class="bar-wrap">
                      <div class="bar bar-receita"
                           [style.height.px]="calcBarH(m.receitasConfirmado)"
                           [title]="'Receita: ' + (m.receitasConfirmado | currency:'BRL':'symbol':'1.2-2':'pt-BR')"></div>
                      <div class="bar bar-despesa"
                           [style.height.px]="calcBarH(m.despesasConfirmado)"
                           [title]="'Despesa: ' + (m.despesasConfirmado | currency:'BRL':'symbol':'1.2-2':'pt-BR')"></div>
                    </div>
                    <span class="bar-label">{{ m.competencia | slice:5 }}/{{ m.competencia | slice:2:4 }}</span>
                  </div>
                }
              </div>
              <div class="chart-legend">
                <span class="legend-dot legend-receita"></span><span>Receita</span>
                <span class="legend-dot legend-despesa" style="margin-left:16px"></span><span>Despesa</span>
              </div>
            </div>
          } @else {
            <p class="empty-state">Nenhum dado disponível</p>
          }
        </div>

        <!-- Sessões por status (donut) -->
        <div class="card">
          <div class="card-header">
            <h3 class="heading-sm">Sessões por status</h3>
          </div>
          <div style="padding:16px">
            @let total = k.totalSessoesAgendadas + k.totalSessoesCanceladas;
            @if (total > 0) {
              <div class="donut-chart">
                <svg viewBox="0 0 120 120" width="120" height="120">
                  <circle cx="60" cy="60" r="50" fill="none" stroke="var(--color-border)" stroke-width="20"/>
                  <!-- Realizadas -->
                  <circle cx="60" cy="60" r="50" fill="none"
                          stroke="var(--color-success)"
                          stroke-width="20"
                          stroke-dasharray="{{ donutDash(k.totalSessoesRealizadas, total) }}"
                          stroke-dashoffset="0"
                          transform="rotate(-90 60 60)"/>
                  <!-- Faltas -->
                  <circle cx="60" cy="60" r="50" fill="none"
                          stroke="var(--color-warning)"
                          stroke-width="20"
                          stroke-dasharray="{{ donutDash(k.totalSessoesFalta + k.totalSessoesFaltaJustificada, total) }}"
                          [style.stroke-dashoffset]="donutOffset(k.totalSessoesRealizadas, total)"
                          transform="rotate(-90 60 60)"/>
                  <text x="60" y="64" text-anchor="middle" font-size="18" font-weight="700"
                        fill="var(--color-text)">{{ k.totalSessoesRealizadas }}</text>
                </svg>
              </div>
              <div style="margin-top:12px">
                <div class="status-row">
                  <span class="dot dot-green"></span>
                  <span class="body-text">Realizadas</span>
                  <span class="ml-auto body-text">{{ k.totalSessoesRealizadas }}</span>
                </div>
                <div class="status-row">
                  <span class="dot dot-yellow"></span>
                  <span class="body-text">Faltas</span>
                  <span class="ml-auto body-text">{{ k.totalSessoesFalta + k.totalSessoesFaltaJustificada }}</span>
                </div>
                <div class="status-row">
                  <span class="dot dot-gray"></span>
                  <span class="body-text">Canceladas</span>
                  <span class="ml-auto body-text">{{ k.totalSessoesCanceladas }}</span>
                </div>
              </div>
            } @else {
              <p class="empty-state">Sem sessões no período</p>
            }
          </div>
        </div>
      </div>

      <!-- ── Linha 3: Ranking + Inadimplentes ── -->
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:20px;margin-bottom:24px">

        <!-- Ranking psicólogos -->
        <div class="card">
          <div class="card-header">
            <h3 class="heading-sm">Ranking — psicólogos</h3>
          </div>
          @if (k.rankingPsicologos.length === 0) {
            <p class="empty-state">Nenhum dado disponível</p>
          } @else {
            <table class="table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Psicólogo</th>
                  <th class="text-right">Sessões</th>
                  <th class="text-right">Receita</th>
                  <th class="text-right">Absent.</th>
                </tr>
              </thead>
              <tbody>
                @for (r of k.rankingPsicologos; track r.psicologoId; let i = $index) {
                  <tr>
                    <td>
                      <span class="rank-badge" [class.gold]="i===0" [class.silver]="i===1" [class.bronze]="i===2">
                        {{ i + 1 }}
                      </span>
                    </td>
                    <td><strong>{{ r.nome }}</strong></td>
                    <td class="text-right">{{ r.sessoesRealizadas }}<small class="text-muted">/{{ r.totalSessoes }}</small></td>
                    <td class="text-right text-success">{{ r.receitaGerada | currency:'BRL':'symbol':'1.0-0':'pt-BR' }}</td>
                    <td class="text-right" [style.color]="r.taxaAbsenteismo > 20 ? 'var(--color-danger)' : 'var(--color-text)'">
                      {{ r.taxaAbsenteismo }}%
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>

        <!-- Pacientes inadimplentes -->
        <div class="card">
          <div class="card-header" style="display:flex;justify-content:space-between;align-items:center">
            <h3 class="heading-sm">Inadimplência</h3>
            @if (k.pacientesInadimplentes.length > 0) {
              <span class="badge badge-danger">{{ k.pacientesInadimplentes.length }} pacientes</span>
            }
          </div>
          @if (k.pacientesInadimplentes.length === 0) {
            <p class="empty-state" style="color:var(--color-success)">✓ Nenhum paciente inadimplente</p>
          } @else {
            <table class="table">
              <thead>
                <tr>
                  <th>Paciente</th>
                  <th class="text-right">Parcelas</th>
                  <th class="text-right">Valor</th>
                  <th>Mais antigo</th>
                </tr>
              </thead>
              <tbody>
                @for (p of k.pacientesInadimplentes; track p.pacienteId) {
                  <tr>
                    <td><strong>{{ p.nome }}</strong></td>
                    <td class="text-right text-danger">{{ p.totalLancamentosVencidos }}</td>
                    <td class="text-right text-danger">{{ p.valorTotal | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</td>
                    <td style="color:var(--color-muted);font-size:12px">
                      {{ p.vencimentoMaisAntigo | date:'dd/MM/yyyy' }}
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>
      </div>

      <!-- ── Gráfico Absenteísmo por psicólogo ── -->
      @if (k.rankingPsicologos.length > 0) {
        <div class="card" style="margin-bottom:24px">
          <div class="card-header">
            <h3 class="heading-sm">Absenteísmo por psicólogo</h3>
          </div>
          <div style="padding:16px 0">
            @for (r of k.rankingPsicologos; track r.psicologoId) {
              <div class="absenteismo-row">
                <span class="absenteismo-nome">{{ r.nome }}</span>
                <div class="absenteismo-bar-wrap">
                  <div class="absenteismo-bar"
                       [style.width.%]="r.taxaAbsenteismo"
                       [style.background]="r.taxaAbsenteismo > 20 ? 'var(--color-danger)' : r.taxaAbsenteismo > 10 ? 'var(--color-warning)' : 'var(--color-success)'">
                  </div>
                </div>
                <span class="absenteismo-pct" [style.color]="r.taxaAbsenteismo > 20 ? 'var(--color-danger)' : 'var(--color-muted)'">
                  {{ r.taxaAbsenteismo }}%
                </span>
              </div>
            }
          </div>
        </div>
      }
    }
  `,
  styles: [`
    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 4px;
    }

    .kpi-card {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      padding: 16px 18px;
      display: flex;
      flex-direction: column;
      gap: 4px;
      transition: box-shadow 0.15s;
      &:hover { box-shadow: 0 2px 8px rgba(0,0,0,.08); }
    }
    .kpi-alert { border-color: var(--color-danger-light, #fecaca); }

    .kpi-label { font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: .06em; color: var(--color-muted); }
    .kpi-value { font-size: 22px; font-weight: 700; color: var(--color-text); line-height: 1.2; }
    .kpi-sub { font-size: 11px; color: var(--color-hint); }

    .kpi-bar { height: 4px; background: var(--color-border); border-radius: 2px; margin-top: 6px; overflow: hidden; }
    .kpi-bar-fill { height: 100%; border-radius: 2px; transition: width 0.4s ease; max-width: 100%; }
    .kpi-bar-green { background: var(--color-success); }
    .kpi-bar-red { background: var(--color-danger); }
    .kpi-bar-blue { background: var(--color-primary-300); }

    .periodo-tabs { display: flex; border: 1px solid var(--color-border); border-radius: var(--radius-md); overflow: hidden; }
    .periodo-tab {
      padding: 6px 14px; font-size: 12px; font-weight: 500; background: none;
      border: none; cursor: pointer; color: var(--color-muted);
      &:hover { background: var(--color-surface-2); }
      &.active { background: var(--color-primary-50); color: var(--color-primary-300); font-weight: 600; }
    }

    .bar-chart { display: flex; align-items: flex-end; gap: 6px; height: 160px; padding: 0 8px; }
    .bar-group { display: flex; flex-direction: column; align-items: center; gap: 4px; flex: 1; }
    .bar-wrap { display: flex; align-items: flex-end; gap: 3px; }
    .bar { width: 14px; min-height: 2px; border-radius: 3px 3px 0 0; transition: height 0.4s ease; }
    .bar-receita { background: var(--color-success); }
    .bar-despesa { background: var(--color-danger); }
    .bar-label { font-size: 10px; color: var(--color-hint); }

    .chart-legend { display: flex; align-items: center; gap: 6px; font-size: 11px; color: var(--color-muted); padding: 0 8px; margin-top: 4px; }
    .legend-dot { width: 10px; height: 10px; border-radius: 50%; display: inline-block; }
    .legend-receita { background: var(--color-success); }
    .legend-despesa { background: var(--color-danger); }

    .donut-chart { display: flex; justify-content: center; }

    .status-row { display: flex; align-items: center; gap: 8px; padding: 6px 0; border-bottom: 1px solid var(--color-border); &:last-child { border-bottom: none; } }
    .dot { width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }
    .dot-green { background: var(--color-success); }
    .dot-yellow { background: var(--color-warning); }
    .dot-gray { background: var(--color-border-2); }
    .ml-auto { margin-left: auto; }

    .rank-badge { display: inline-flex; align-items: center; justify-content: center; width: 22px; height: 22px; border-radius: 50%; font-size: 11px; font-weight: 700; background: var(--color-surface-2); color: var(--color-muted); }
    .rank-badge.gold { background: #fef3c7; color: #d97706; }
    .rank-badge.silver { background: #f1f5f9; color: #64748b; }
    .rank-badge.bronze { background: #fef2e6; color: #c2721f; }
    .text-muted { color: var(--color-muted); font-size: 11px; }

    .absenteismo-row { display: flex; align-items: center; gap: 12px; padding: 8px 16px; }
    .absenteismo-nome { width: 140px; font-size: 13px; font-weight: 500; color: var(--color-text); flex-shrink: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .absenteismo-bar-wrap { flex: 1; height: 8px; background: var(--color-border); border-radius: 4px; overflow: hidden; }
    .absenteismo-bar { height: 100%; border-radius: 4px; transition: width 0.4s ease; max-width: 100%; }
    .absenteismo-pct { width: 44px; text-align: right; font-size: 12px; font-weight: 600; }

    .badge-danger { background: #fee2e2; color: #dc2626; }
    .loading-overlay { display: flex; justify-content: center; align-items: center; padding: 60px; }
    .loading-spinner-lg { width: 32px; height: 32px; border: 3px solid var(--color-border); border-top-color: var(--color-primary-300); border-radius: 50%; animation: spin 0.8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
})
export class DashboardComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly kpis = signal<KpisDashboard | null>(null);
  readonly fluxo = signal<FluxoMes[]>([]);
  readonly loading = signal(false);
  readonly periodo = signal<PeriodoFiltro>('mes');

  readonly periodos: { value: PeriodoFiltro; label: string }[] = [
    { value: 'mes', label: 'Mês' },
    { value: 'trimestre', label: 'Trimestre' },
    { value: 'semestre', label: 'Semestre' },
    { value: 'ano', label: 'Ano' },
  ];

  competencia = signal(this.mesAtual());

  private maxFluxo = 0;

  ngOnInit(): void {
    this.carregar();
  }

  onCompetenciaChange(value: string): void {
    this.competencia.set(value);
    this.carregar();
  }

  setPeriodo(p: PeriodoFiltro): void {
    this.periodo.set(p);
  }

  carregar(): void {
    this.loading.set(true);
    const comp = this.competencia();

    this.api.get<KpisDashboard>('dashboard/kpis', { competencia: comp })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.kpis.set(data); this.loading.set(false); },
        error: () => this.loading.set(false),
      });

    // Fluxo 6 meses
    const fim = comp;
    const inicio = this.subtrairMeses(comp, 5);
    this.api.get<FluxoMes[]>('dashboard/relatorios/fluxo-caixa', { inicio, fim })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.maxFluxo = Math.max(...data.flatMap(m => [m.receitasConfirmado, m.despesasConfirmado]), 1);
          this.fluxo.set(data);
        },
        error: () => {},
      });
  }

  calcBarH(valor: number): number {
    return this.maxFluxo > 0 ? Math.round((valor / this.maxFluxo) * 140) : 2;
  }

  donutDash(parte: number, total: number): string {
    const circ = 2 * Math.PI * 50;
    const frac = total > 0 ? parte / total : 0;
    return `${frac * circ} ${circ}`;
  }

  donutOffset(antes: number, total: number): string {
    const circ = 2 * Math.PI * 50;
    const frac = total > 0 ? antes / total : 0;
    return `${-frac * circ}`;
  }

  private mesAtual(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }

  private subtrairMeses(comp: string, meses: number): string {
    const [ano, mes] = comp.split('-').map(Number);
    const d = new Date(ano, mes - 1 - meses, 1);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }
}
