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
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormatoExportacao,
  RelatorioPersonalizadoDto,
  RelatorioTemplateDto,
  RelatoriosBIService,
  TipoRelatorio,
} from '../relatorios-bi.service';

interface TipoOpcao {
  valor: TipoRelatorio | null;
  label: string;
}

@Component({
  selector: 'app-relatorios-lista',
  standalone: true,
  imports: [CommonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container">
      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="heading-lg">Relatórios B.I.</h1>
          <p class="body-sm text-muted">Analise dados da clínica com relatórios personalizados</p>
        </div>
        <a routerLink="novo" class="btn btn--primary">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Novo Relatório
        </a>
      </div>

      <!-- Filtro por tipo -->
      <div class="filter-bar">
        @for (opcao of tiposOpcoes; track opcao.valor) {
          <button
            class="filter-chip"
            [class.filter-chip--active]="filtroTipo() === opcao.valor"
            (click)="filtroTipo.set(opcao.valor)">
            {{ opcao.label }}
          </button>
        }
      </div>

      <!-- Tabs -->
      <div class="tabs">
        <button class="tab" [class.tab--active]="aba() === 'templates'" (click)="aba.set('templates')">
          Templates Pré-definidos
          <span class="badge">{{ templatesFiltrados().length }}</span>
        </button>
        <button class="tab" [class.tab--active]="aba() === 'personalizados'" (click)="aba.set('personalizados')">
          Meus Relatórios
          <span class="badge">{{ personalizadosFiltrados().length }}</span>
        </button>
      </div>

      @if (carregando()) {
        <div class="spinner-container">
          <div class="spinner"></div>
        </div>
      } @else {

        <!-- Templates -->
        @if (aba() === 'templates') {
          @if (templatesFiltrados().length === 0) {
            <div class="empty-state">
              <p>Nenhum template encontrado para o tipo selecionado.</p>
            </div>
          } @else {
            <div class="cards-grid">
              @for (tpl of templatesFiltrados(); track tpl.id) {
                <div class="card">
                  <div class="card__icon" [style.background]="corPorTipo(tpl.tipo)">
                    <span [innerHTML]="iconePorTipo(tpl.tipo)"></span>
                  </div>
                  <div class="card__body">
                    <h3 class="card__title">{{ tpl.nome }}</h3>
                    <p class="card__desc">{{ tpl.descricao }}</p>
                    <span class="badge badge--tipo">{{ labelTipo(tpl.tipo) }}</span>
                  </div>
                  <div class="card__actions">
                    <button class="btn btn--sm btn--primary" (click)="executarTemplate(tpl)">
                      Executar
                    </button>
                    <button class="btn btn--sm btn--outline" (click)="personalizarTemplate(tpl)">
                      Personalizar
                    </button>
                  </div>
                </div>
              }
            </div>
          }
        }

        <!-- Meus Relatórios -->
        @if (aba() === 'personalizados') {
          @if (personalizadosFiltrados().length === 0) {
            <div class="empty-state">
              <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="var(--color-hint)" stroke-width="1.5"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
              <p>Nenhum relatório personalizado criado ainda.</p>
              <a routerLink="novo" class="btn btn--primary btn--sm">Criar primeiro relatório</a>
            </div>
          } @else {
            <div class="table-container">
              <table class="table">
                <thead>
                  <tr>
                    <th>Nome</th>
                    <th>Tipo</th>
                    <th>Agrupamento</th>
                    <th>Favorito</th>
                    <th>Criado em</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  @for (rel of personalizadosFiltrados(); track rel.id) {
                    <tr>
                      <td>
                        <span class="font-medium">{{ rel.nome }}</span>
                        @if (rel.descricao) {
                          <p class="text-sm text-muted">{{ rel.descricao }}</p>
                        }
                      </td>
                      <td>
                        <span class="badge badge--tipo">{{ labelTipo(rel.tipo) }}</span>
                      </td>
                      <td>{{ rel.agrupamento ?? '—' }}</td>
                      <td>
                        <button
                          class="btn-icon"
                          [class.btn-icon--active]="rel.favorito"
                          (click)="toggleFavorito(rel)"
                          [title]="rel.favorito ? 'Remover dos favoritos' : 'Adicionar aos favoritos'">
                          <svg width="18" height="18" viewBox="0 0 24 24"
                            [attr.fill]="rel.favorito ? 'currentColor' : 'none'"
                            stroke="currentColor" stroke-width="2">
                            <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
                          </svg>
                        </button>
                      </td>
                      <td class="text-sm">{{ rel.criadoEm | date:'dd/MM/yyyy' }}</td>
                      <td>
                        <div class="actions-row">
                          <button class="btn btn--sm btn--primary" [routerLink]="[rel.id, 'visualizar']">
                            Ver
                          </button>
                          <button class="btn btn--sm btn--outline" [routerLink]="[rel.id, 'editar']">
                            Editar
                          </button>
                          <button class="btn btn--sm btn--danger" (click)="excluir(rel)">
                            Excluir
                          </button>
                        </div>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }
      }
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 1200px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .filter-bar { display: flex; gap: 8px; flex-wrap: wrap; margin-bottom: 16px; }
    .filter-chip {
      padding: 6px 14px; border-radius: 20px; border: 1px solid var(--color-border);
      background: var(--color-surface); font-size: 13px; cursor: pointer; transition: all .15s;
      &:hover { background: var(--color-surface-2); }
      &--active { background: var(--color-primary-50); border-color: var(--color-primary-300); color: var(--color-primary-300); font-weight: 600; }
    }
    .tabs { display: flex; gap: 0; border-bottom: 1px solid var(--color-border); margin-bottom: 24px; }
    .tab {
      padding: 10px 20px; font-size: 14px; font-weight: 500; color: var(--color-muted);
      border: none; background: none; cursor: pointer; border-bottom: 2px solid transparent; margin-bottom: -1px;
      display: flex; align-items: center; gap: 8px; transition: all .15s;
      &:hover { color: var(--color-text); }
      &--active { color: var(--color-primary-300); border-bottom-color: var(--color-primary-300); }
    }
    .cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 16px; }
    .card {
      background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-md);
      padding: 16px; display: flex; flex-direction: column; gap: 12px; transition: box-shadow .15s;
      &:hover { box-shadow: var(--shadow-md); }
    }
    .card__icon { width: 40px; height: 40px; border-radius: 10px; display: flex; align-items: center; justify-content: center; color: white; flex-shrink: 0; }
    .card__title { font-size: 15px; font-weight: 600; color: var(--color-text); margin: 0 0 4px; }
    .card__desc { font-size: 13px; color: var(--color-muted); margin: 0; line-height: 1.5; }
    .card__actions { display: flex; gap: 8px; margin-top: auto; }
    .table-container { overflow-x: auto; }
    .table { width: 100%; border-collapse: collapse; font-size: 14px; }
    .table th { text-align: left; padding: 10px 12px; border-bottom: 1px solid var(--color-border); color: var(--color-muted); font-weight: 600; font-size: 12px; text-transform: uppercase; letter-spacing: .05em; }
    .table td { padding: 12px; border-bottom: 1px solid var(--color-border-light, #f0f0f0); vertical-align: middle; }
    .table tr:hover td { background: var(--color-surface-2); }
    .actions-row { display: flex; gap: 6px; }
    .btn-icon { background: none; border: none; cursor: pointer; padding: 4px; border-radius: 4px; color: var(--color-muted); transition: all .15s; &:hover { color: var(--color-text); background: var(--color-surface-2); } &--active { color: #f59e0b; } }
    .badge--tipo { font-size: 11px; padding: 2px 8px; border-radius: 12px; background: var(--color-primary-50); color: var(--color-primary-300); }
    .empty-state { display: flex; flex-direction: column; align-items: center; gap: 12px; padding: 48px; color: var(--color-hint); text-align: center; }
    .spinner-container { display: flex; justify-content: center; padding: 48px; }
    .spinner { width: 36px; height: 36px; border: 3px solid var(--color-border); border-top-color: var(--color-primary-300); border-radius: 50%; animation: spin 0.8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .btn--danger { background: #fee2e2; color: #dc2626; border-color: #fca5a5; &:hover { background: #fecaca; } }
    .font-medium { font-weight: 500; }
    .text-sm { font-size: 12px; }
    .text-muted { color: var(--color-muted); }
  `]
})
export class RelatoriosListaComponent implements OnInit {
  private readonly service = inject(RelatoriosBIService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  templates = signal<RelatorioTemplateDto[]>([]);
  personalizados = signal<RelatorioPersonalizadoDto[]>([]);
  carregando = signal(false);
  filtroTipo = signal<TipoRelatorio | null>(null);
  aba = signal<'templates' | 'personalizados'>('templates');

  templatesFiltrados = computed(() => {
    const tipo = this.filtroTipo();
    return tipo === null
      ? this.templates()
      : this.templates().filter((t) => t.tipo === tipo);
  });

  personalizadosFiltrados = computed(() => {
    const tipo = this.filtroTipo();
    return tipo === null
      ? this.personalizados()
      : this.personalizados().filter((r) => r.tipo === tipo);
  });

  readonly tiposOpcoes: TipoOpcao[] = [
    { valor: null, label: 'Todos' },
    { valor: TipoRelatorio.Financeiro, label: 'Financeiro' },
    { valor: TipoRelatorio.Sessoes, label: 'Sessões' },
    { valor: TipoRelatorio.Pacientes, label: 'Pacientes' },
    { valor: TipoRelatorio.Psicologos, label: 'Psicólogos' },
    { valor: TipoRelatorio.Inadimplencia, label: 'Inadimplência' },
    { valor: TipoRelatorio.Repasses, label: 'Repasses' },
    { valor: TipoRelatorio.FluxoCaixaProjetado, label: 'Fluxo Caixa' },
    { valor: TipoRelatorio.Comparativo, label: 'Comparativo' },
  ];

  ngOnInit(): void {
    this.carregando.set(true);

    this.service
      .obterTemplates()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((templates) => this.templates.set(templates));

    this.service
      .listar()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (lista) => {
          this.personalizados.set(lista);
          this.carregando.set(false);
        },
        error: () => this.carregando.set(false),
      });
  }

  executarTemplate(tpl: RelatorioTemplateDto): void {
    this.router.navigate(['/relatorios-bi/executar'], {
      state: { template: tpl },
    });
  }

  personalizarTemplate(tpl: RelatorioTemplateDto): void {
    this.router.navigate(['/relatorios-bi/novo'], {
      state: { template: tpl },
    });
  }

  toggleFavorito(rel: RelatorioPersonalizadoDto): void {
    const novoValor = !rel.favorito;
    this.service
      .marcarFavorito(rel.id, novoValor)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.personalizados.update((lista) =>
          lista.map((r) => (r.id === rel.id ? { ...r, favorito: novoValor } : r))
        );
      });
  }

  excluir(rel: RelatorioPersonalizadoDto): void {
    if (!confirm(`Excluir o relatório "${rel.nome}"?`)) return;
    this.service
      .excluir(rel.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.personalizados.update((lista) => lista.filter((r) => r.id !== rel.id));
      });
  }

  labelTipo(tipo: TipoRelatorio): string {
    const mapa: Record<TipoRelatorio, string> = {
      [TipoRelatorio.Financeiro]: 'Financeiro',
      [TipoRelatorio.Sessoes]: 'Sessões',
      [TipoRelatorio.Pacientes]: 'Pacientes',
      [TipoRelatorio.Psicologos]: 'Psicólogos',
      [TipoRelatorio.Inadimplencia]: 'Inadimplência',
      [TipoRelatorio.Repasses]: 'Repasses',
      [TipoRelatorio.FluxoCaixaProjetado]: 'Fluxo de Caixa',
      [TipoRelatorio.Comparativo]: 'Comparativo',
    };
    return mapa[tipo] ?? 'N/A';
  }

  corPorTipo(tipo: TipoRelatorio): string {
    const cores: Record<TipoRelatorio, string> = {
      [TipoRelatorio.Financeiro]: '#3f51b5',
      [TipoRelatorio.Sessoes]: '#009688',
      [TipoRelatorio.Pacientes]: '#4caf50',
      [TipoRelatorio.Psicologos]: '#9c27b0',
      [TipoRelatorio.Inadimplencia]: '#f44336',
      [TipoRelatorio.Repasses]: '#ff9800',
      [TipoRelatorio.FluxoCaixaProjetado]: '#2196f3',
      [TipoRelatorio.Comparativo]: '#607d8b',
    };
    return cores[tipo] ?? '#607d8b';
  }

  iconePorTipo(tipo: TipoRelatorio): string {
    const icones: Record<TipoRelatorio, string> = {
      [TipoRelatorio.Financeiro]: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>',
      [TipoRelatorio.Sessoes]: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>',
      [TipoRelatorio.Pacientes]: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>',
      [TipoRelatorio.Psicologos]: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>',
      [TipoRelatorio.Inadimplencia]: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>',
      [TipoRelatorio.Repasses]: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="17 1 21 5 17 9"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><polyline points="7 23 3 19 7 15"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg>',
      [TipoRelatorio.FluxoCaixaProjetado]: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>',
      [TipoRelatorio.Comparativo]: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/></svg>',
    };
    return icones[tipo] ?? '';
  }
}
