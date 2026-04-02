import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ExecutarRelatorioRequest,
  FormatoExportacao,
  RelatorioResultadoDto,
  RelatorioTemplateDto,
  RelatoriosBIService,
  TipoRelatorio,
} from '../relatorios-bi.service';

type VisualizacaoTipo = 'tabela' | 'grafico';

interface GraficoConfiguracao {
  tipo: string;
  labels: string[];
  datasets: GraficoDataset[];
}

interface GraficoDataset {
  label: string;
  data: number[];
  backgroundColor: string | string[];
  borderColor?: string | string[];
}

@Component({
  selector: 'app-relatorio-visualizador',
  standalone: true,
  imports: [CommonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container">
      <!-- Breadcrumb -->
      <nav class="breadcrumb">
        <a routerLink="/relatorios-bi">Relatórios B.I.</a>
        <span>/</span>
        <span>{{ resultado()?.titulo ?? 'Visualizador' }}</span>
      </nav>

      @if (carregando()) {
        <div class="spinner-container">
          <div class="spinner"></div>
          <p class="text-muted">Executando relatório...</p>
        </div>
      } @else if (!resultado()) {
        <div class="empty-state">
          <p>Nenhum resultado disponível. Execute um relatório.</p>
          <a routerLink="/relatorios-bi" class="btn btn--outline btn--sm">Voltar</a>
        </div>
      } @else {
        <!-- Header do resultado -->
        <div class="result-header">
          <div>
            <h1 class="heading-lg">{{ resultado()!.titulo }}</h1>
            @if (resultado()!.descricao) {
              <p class="text-muted">{{ resultado()!.descricao }}</p>
            }
            <p class="text-sm text-hint">
              {{ resultado()!.totalRegistros }} registros &bull;
              Gerado em {{ resultado()!.geradoEm | date:'dd/MM/yyyy HH:mm' }}
            </p>
          </div>

          <!-- Exportação -->
          <div class="export-buttons">
            <button class="btn btn--outline btn--sm" (click)="exportar(FormatoExportacao.Pdf)" [disabled]="exportando()">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
              PDF
            </button>
            <button class="btn btn--outline btn--sm" (click)="exportar(FormatoExportacao.Xlsx)" [disabled]="exportando()">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="3" width="20" height="14" rx="2"/><line x1="8" y1="21" x2="16" y2="21"/><line x1="12" y1="17" x2="12" y2="21"/></svg>
              XLSX
            </button>
            <button class="btn btn--outline btn--sm" (click)="exportar(FormatoExportacao.Csv)" [disabled]="exportando()">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>
              CSV
            </button>
          </div>
        </div>

        <!-- Tabs de visualização -->
        <div class="tabs">
          <button class="tab" [class.tab--active]="tipoVisualizacao() === 'tabela'" (click)="tipoVisualizacao.set('tabela')">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><line x1="3" y1="9" x2="21" y2="9"/><line x1="3" y1="15" x2="21" y2="15"/><line x1="9" y1="3" x2="9" y2="21"/></svg>
            Tabela
          </button>
          <button class="tab" [class.tab--active]="tipoVisualizacao() === 'grafico'" (click)="tipoVisualizacao.set('grafico')">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/></svg>
            Gráfico
          </button>
        </div>

        <!-- Tabela dinâmica -->
        @if (tipoVisualizacao() === 'tabela') {
          <div class="table-container">
            @if (resultado()!.linhas.length === 0) {
              <div class="empty-state">
                <p>Nenhum registro encontrado para os filtros aplicados.</p>
              </div>
            } @else {
              <table class="data-table">
                <thead>
                  <tr>
                    @for (col of resultado()!.colunas; track col) {
                      <th>{{ col }}</th>
                    }
                  </tr>
                </thead>
                <tbody>
                  @for (linha of resultado()!.linhas; track $index) {
                    <tr>
                      @for (col of resultado()!.colunas; track col) {
                        <td [class.td-number]="isNumero(linha[col])">
                          {{ formatarValor(linha[col]) }}
                        </td>
                      }
                    </tr>
                  }
                </tbody>
              </table>
            }
          </div>
          <p class="table-footer">Total: {{ resultado()!.totalRegistros }} registros</p>
        }

        <!-- Gráfico (representação textual quando Chart.js não está instalado) -->
        @if (tipoVisualizacao() === 'grafico') {
          <div class="chart-container">
            <div class="chart-aviso">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="var(--color-primary-300)" stroke-width="2"><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/></svg>
              <p class="font-medium">Visualização em Gráfico</p>
              <p class="text-sm text-muted">
                Para habilitar gráficos interativos, instale ng2-charts e chart.js:
                <code>npm install ng2-charts chart.js</code>
              </p>
            </div>

            <!-- Preview bar-chart simplificado com CSS -->
            @if (colunaNumericaPrincipal()) {
              <div class="bar-chart">
                <p class="bar-chart__titulo">{{ colunaNumericaPrincipal() }}</p>
                <div class="bar-chart__linhas">
                  @for (linha of resultado()!.linhas.slice(0, 10); track $index) {
                    <div class="bar-chart__linha">
                      <span class="bar-chart__label">{{ linha[resultado()!.colunas[0]] }}</span>
                      <div class="bar-chart__barra-container">
                        <div
                          class="bar-chart__barra"
                          [style.width.%]="calcularLarguraBarra(linha[colunaNumericaPrincipal()!])">
                        </div>
                        <span class="bar-chart__valor">{{ formatarValor(linha[colunaNumericaPrincipal()!]) }}</span>
                      </div>
                    </div>
                  }
                </div>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 1200px; margin: 0 auto; }
    .breadcrumb { display: flex; gap: 8px; align-items: center; font-size: 13px; color: var(--color-muted); margin-bottom: 16px; a { color: var(--color-primary-300); text-decoration: none; &:hover { text-decoration: underline; } } }
    .result-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 16px; margin-bottom: 20px; flex-wrap: wrap; }
    .export-buttons { display: flex; gap: 8px; flex-wrap: wrap; align-items: flex-start; }
    .tabs { display: flex; gap: 0; border-bottom: 1px solid var(--color-border); margin-bottom: 20px; }
    .tab { padding: 10px 20px; font-size: 14px; font-weight: 500; color: var(--color-muted); border: none; background: none; cursor: pointer; border-bottom: 2px solid transparent; margin-bottom: -1px; display: flex; align-items: center; gap: 8px; transition: all .15s; &:hover { color: var(--color-text); } &--active { color: var(--color-primary-300); border-bottom-color: var(--color-primary-300); } }
    .table-container { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 14px; }
    .data-table th { text-align: left; padding: 10px 12px; background: var(--color-surface-2); border-bottom: 2px solid var(--color-border); font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: .04em; color: var(--color-muted); white-space: nowrap; }
    .data-table td { padding: 11px 12px; border-bottom: 1px solid var(--color-border-light, #f0f0f0); vertical-align: middle; }
    .data-table tr:hover td { background: var(--color-surface-2); }
    .td-number { text-align: right; font-variant-numeric: tabular-nums; }
    .table-footer { font-size: 12px; color: var(--color-hint); margin-top: 8px; text-align: right; }
    .chart-container { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-md); padding: 24px; display: flex; flex-direction: column; gap: 24px; }
    .chart-aviso { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 16px; background: var(--color-primary-50); border-radius: var(--radius-sm); text-align: center; code { background: var(--color-surface); padding: 4px 8px; border-radius: 4px; font-size: 12px; border: 1px solid var(--color-border); display: block; margin-top: 8px; } }
    .bar-chart { }
    .bar-chart__titulo { font-size: 13px; font-weight: 600; color: var(--color-muted); margin-bottom: 12px; text-transform: uppercase; letter-spacing: .05em; }
    .bar-chart__linhas { display: flex; flex-direction: column; gap: 10px; }
    .bar-chart__linha { display: flex; align-items: center; gap: 12px; }
    .bar-chart__label { min-width: 120px; font-size: 13px; color: var(--color-text); text-align: right; flex-shrink: 0; }
    .bar-chart__barra-container { flex: 1; display: flex; align-items: center; gap: 8px; }
    .bar-chart__barra { height: 24px; background: var(--color-primary-300); border-radius: 4px; min-width: 4px; transition: width .4s ease; }
    .bar-chart__valor { font-size: 12px; color: var(--color-muted); white-space: nowrap; }
    .spinner-container { display: flex; flex-direction: column; align-items: center; gap: 12px; padding: 64px; }
    .spinner { width: 36px; height: 36px; border: 3px solid var(--color-border); border-top-color: var(--color-primary-300); border-radius: 50%; animation: spin 0.8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .empty-state { display: flex; flex-direction: column; align-items: center; gap: 12px; padding: 48px; color: var(--color-hint); text-align: center; }
    .text-muted { color: var(--color-muted); }
    .text-sm { font-size: 13px; }
    .text-hint { color: var(--color-hint); font-size: 12px; }
    .font-medium { font-weight: 500; }
  `]
})
export class RelatorioVisualizadorComponent implements OnInit {
  private readonly service = inject(RelatoriosBIService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly FormatoExportacao = FormatoExportacao;

  resultado = signal<RelatorioResultadoDto | null>(null);
  carregando = signal(false);
  tipoVisualizacao = signal<VisualizacaoTipo>('tabela');
  exportando = signal(false);

  private relatorioId: string | null = null;

  valorMaximo = computed(() => {
    const res = this.resultado();
    if (!res) return 1;
    const coluna = this.colunaNumericaPrincipal();
    if (!coluna) return 1;
    const valores = res.linhas.map((l) => this.toNumero(l[coluna]));
    return Math.max(...valores, 1);
  });

  ngOnInit(): void {
    this.relatorioId = this.route.snapshot.paramMap.get('id');
    const navState = history.state as {
      resultado?: RelatorioResultadoDto;
      template?: RelatorioTemplateDto;
    };

    if (navState?.resultado) {
      this.resultado.set(navState.resultado);
      return;
    }

    if (navState?.template) {
      this.executarTemplate(navState.template);
      return;
    }

    if (this.relatorioId) {
      this.executarSalvo(this.relatorioId);
    }
  }

  private executarSalvo(id: string): void {
    this.carregando.set(true);
    this.service
      .executarSalvo(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.resultado.set(res);
          this.carregando.set(false);
        },
        error: () => this.carregando.set(false),
      });
  }

  private executarTemplate(tpl: RelatorioTemplateDto): void {
    this.carregando.set(true);
    const request: ExecutarRelatorioRequest = {
      tipo: tpl.tipo,
      filtros: tpl.filtrosPadrao,
      agrupamento: tpl.agrupamento,
    };
    this.service
      .executarAdHoc(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.resultado.set(res);
          this.carregando.set(false);
        },
        error: () => this.carregando.set(false),
      });
  }

  exportar(formato: FormatoExportacao): void {
    if (!this.resultado()) return;
    this.exportando.set(true);

    const obs = this.relatorioId
      ? this.service.exportarSalvo(this.relatorioId, formato)
      : this.service.exportarAdHoc({
          tipo: TipoRelatorio.Financeiro,
          filtros: {},
          formato,
        });

    obs.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (blob) => {
        this.downloadBlob(blob, formato);
        this.exportando.set(false);
      },
      error: () => this.exportando.set(false),
    });
  }

  private downloadBlob(blob: Blob, formato: FormatoExportacao): void {
    const titulo = this.resultado()?.titulo ?? 'relatorio';
    const nomeBase = titulo.replace(/\s+/g, '_').replace(/\//g, '-');
    const extensoes: Record<FormatoExportacao, string> = {
      [FormatoExportacao.Json]: 'json',
      [FormatoExportacao.Pdf]: 'pdf',
      [FormatoExportacao.Xlsx]: 'xlsx',
      [FormatoExportacao.Csv]: 'csv',
    };
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${nomeBase}.${extensoes[formato]}`;
    a.click();
    URL.revokeObjectURL(url);
  }

  colunaNumericaPrincipal(): string | null {
    const res = this.resultado();
    if (!res || res.colunas.length < 2) return null;
    const numericas = res.colunas.filter((col, idx) =>
      idx > 0 && res.linhas.some((l) => typeof l[col] === 'number')
    );
    return numericas[0] ?? null;
  }

  calcularLarguraBarra(valor: unknown): number {
    const num = this.toNumero(valor);
    const max = this.valorMaximo();
    return max > 0 ? Math.round((num / max) * 100) : 0;
  }

  isNumero(valor: unknown): boolean {
    return typeof valor === 'number';
  }

  formatarValor(valor: unknown): string {
    if (valor === null || valor === undefined) return '—';
    if (typeof valor === 'number') {
      return Number.isInteger(valor) ? valor.toString() : valor.toFixed(2);
    }
    return String(valor);
  }

  private toNumero(valor: unknown): number {
    if (typeof valor === 'number') return valor;
    if (typeof valor === 'string') {
      const n = parseFloat(valor.replace(',', '.'));
      return isNaN(n) ? 0 : n;
    }
    return 0;
  }
}
