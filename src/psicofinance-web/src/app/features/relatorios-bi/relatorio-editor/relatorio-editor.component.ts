import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  CriarRelatorioCommand,
  ExecutarRelatorioRequest,
  RelatorioFiltrosDto,
  RelatorioResultadoDto,
  RelatorioTemplateDto,
  RelatoriosBIService,
  StatusLancamento,
  StatusSessao,
  TipoLancamento,
  TipoRelatorio,
} from '../relatorios-bi.service';

@Component({
  selector: 'app-relatorio-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-container">
      <!-- Breadcrumb -->
      <nav class="breadcrumb">
        <a routerLink="/relatorios-bi">Relatórios B.I.</a>
        <span>/</span>
        <span>{{ editando() ? 'Editar Relatório' : 'Novo Relatório' }}</span>
      </nav>

      <h1 class="heading-lg mb-24">{{ editando() ? 'Editar Relatório' : 'Novo Relatório Personalizado' }}</h1>

      <form [formGroup]="form" (ngSubmit)="salvar()" class="editor-form">

        <!-- Informações básicas -->
        <section class="form-section">
          <h2 class="section-title">Informações Básicas</h2>

          <div class="form-row">
            <div class="form-field">
              <label class="form-label">Nome *</label>
              <input formControlName="nome" class="form-input" placeholder="Ex: Faturamento Mensal 2026" />
              @if (form.get('nome')?.invalid && form.get('nome')?.touched) {
                <span class="form-error">Nome é obrigatório (2-100 caracteres).</span>
              }
            </div>
            <div class="form-field">
              <label class="form-label">Tipo de Relatório *</label>
              <select formControlName="tipo" class="form-select" [disabled]="editando()">
                <option [ngValue]="null" disabled>Selecione...</option>
                @for (opcao of tiposOpcoes; track opcao.valor) {
                  <option [ngValue]="opcao.valor">{{ opcao.label }}</option>
                }
              </select>
            </div>
          </div>

          <div class="form-field">
            <label class="form-label">Descrição</label>
            <textarea formControlName="descricao" class="form-textarea" rows="2" placeholder="Descreva o objetivo deste relatório..."></textarea>
          </div>

          <div class="form-row">
            <div class="form-field">
              <label class="form-label">Agrupamento</label>
              <select formControlName="agrupamento" class="form-select">
                <option [ngValue]="null">Sem agrupamento</option>
                <option value="dia">Por dia</option>
                <option value="semana">Por semana</option>
                <option value="mes">Por mês</option>
                <option value="trimestre">Por trimestre</option>
                <option value="ano">Por ano</option>
              </select>
            </div>
            <div class="form-field">
              <label class="form-label">Ordenação</label>
              <select formControlName="ordenacao" class="form-select">
                <option [ngValue]="null">Padrão</option>
                <option value="asc">Crescente (asc)</option>
                <option value="desc">Decrescente (desc)</option>
              </select>
            </div>
          </div>
        </section>

        <!-- Filtros -->
        <section class="form-section" [formGroup]="filtrosGroup">
          <h2 class="section-title">Filtros</h2>

          <div class="form-row">
            <div class="form-field">
              <label class="form-label">Data Início</label>
              <input type="date" formControlName="dataInicio" class="form-input" />
            </div>
            <div class="form-field">
              <label class="form-label">Data Fim</label>
              <input type="date" formControlName="dataFim" class="form-input" />
            </div>
            <div class="form-field">
              <label class="form-label">Competência (AAAA-MM)</label>
              <input type="month" formControlName="competencia" class="form-input" />
            </div>
          </div>

          @if (exibirFiltroPsicologo()) {
            <div class="form-row">
              <div class="form-field">
                <label class="form-label">Psicólogo</label>
                <input formControlName="psicologoId" class="form-input" placeholder="ID do psicólogo (UUID)" />
              </div>
            </div>
          }

          @if (exibirFiltroPaciente()) {
            <div class="form-row">
              <div class="form-field">
                <label class="form-label">Paciente</label>
                <input formControlName="pacienteId" class="form-input" placeholder="ID do paciente (UUID)" />
              </div>
            </div>
          }

          @if (exibirFiltroStatusSessao()) {
            <div class="form-row">
              <div class="form-field">
                <label class="form-label">Status da Sessão</label>
                <select formControlName="statusSessao" class="form-select">
                  <option [ngValue]="null">Todos</option>
                  <option [ngValue]="StatusSessao.Agendada">Agendada</option>
                  <option [ngValue]="StatusSessao.Realizada">Realizada</option>
                  <option [ngValue]="StatusSessao.Falta">Falta</option>
                  <option [ngValue]="StatusSessao.FaltaJustificada">Falta Justificada</option>
                  <option [ngValue]="StatusSessao.Cancelada">Cancelada</option>
                </select>
              </div>
            </div>
          }

          @if (exibirFiltroLancamento()) {
            <div class="form-row">
              <div class="form-field">
                <label class="form-label">Status do Lançamento</label>
                <select formControlName="statusLancamento" class="form-select">
                  <option [ngValue]="null">Todos</option>
                  <option [ngValue]="StatusLancamento.Previsto">Previsto</option>
                  <option [ngValue]="StatusLancamento.Confirmado">Confirmado</option>
                  <option [ngValue]="StatusLancamento.Cancelado">Cancelado</option>
                </select>
              </div>
              <div class="form-field">
                <label class="form-label">Tipo de Lançamento</label>
                <select formControlName="tipoLancamento" class="form-select">
                  <option [ngValue]="null">Todos</option>
                  <option [ngValue]="TipoLancamento.Receita">Receita</option>
                  <option [ngValue]="TipoLancamento.Despesa">Despesa</option>
                </select>
              </div>
            </div>
          }
        </section>

        <!-- Preview -->
        @if (previewResultado()) {
          <section class="form-section preview-section">
            <h2 class="section-title">Preview ({{ previewResultado()!.totalRegistros }} registros)</h2>
            <p class="text-sm text-muted mb-12">{{ previewResultado()!.titulo }}</p>
            <div class="table-container">
              <table class="preview-table">
                <thead>
                  <tr>
                    @for (col of previewResultado()!.colunas; track col) {
                      <th>{{ col }}</th>
                    }
                  </tr>
                </thead>
                <tbody>
                  @for (linha of previewResultado()!.linhas.slice(0, 5); track $index) {
                    <tr>
                      @for (col of previewResultado()!.colunas; track col) {
                        <td>{{ linha[col] ?? '—' }}</td>
                      }
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            @if (previewResultado()!.totalRegistros > 5) {
              <p class="text-sm text-muted mt-8">Exibindo 5 de {{ previewResultado()!.totalRegistros }} registros.</p>
            }
          </section>
        }

        <!-- Ações -->
        <div class="form-actions">
          <button type="button" class="btn btn--outline" routerLink="/relatorios-bi">Cancelar</button>
          <button type="button" class="btn btn--secondary" (click)="testarFiltros()" [disabled]="testando()">
            @if (testando()) { Executando... } @else { Testar Filtros }
          </button>
          <button type="submit" class="btn btn--primary" [disabled]="form.invalid || salvando()">
            @if (salvando()) { Salvando... } @else { Salvar Relatório }
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 900px; margin: 0 auto; }
    .breadcrumb { display: flex; gap: 8px; align-items: center; font-size: 13px; color: var(--color-muted); margin-bottom: 16px; a { color: var(--color-primary-300); text-decoration: none; &:hover { text-decoration: underline; } } }
    .mb-24 { margin-bottom: 24px; }
    .mb-12 { margin-bottom: 12px; }
    .mt-8 { margin-top: 8px; }
    .editor-form { display: flex; flex-direction: column; gap: 24px; }
    .form-section { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-md); padding: 20px; display: flex; flex-direction: column; gap: 16px; }
    .section-title { font-size: 15px; font-weight: 600; color: var(--color-text); margin: 0 0 4px; }
    .form-row { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 16px; }
    .form-field { display: flex; flex-direction: column; gap: 6px; }
    .form-label { font-size: 13px; font-weight: 500; color: var(--color-text); }
    .form-input, .form-select, .form-textarea { padding: 8px 12px; border: 1px solid var(--color-border); border-radius: var(--radius-sm); font-size: 14px; background: var(--color-surface); color: var(--color-text); width: 100%; box-sizing: border-box; transition: border-color .15s; &:focus { outline: none; border-color: var(--color-primary-300); } }
    .form-textarea { resize: vertical; }
    .form-error { font-size: 12px; color: #dc2626; }
    .form-actions { display: flex; gap: 12px; justify-content: flex-end; padding-top: 8px; }
    .preview-section { border-color: var(--color-primary-100); background: var(--color-primary-50); }
    .table-container { overflow-x: auto; }
    .preview-table { width: 100%; border-collapse: collapse; font-size: 13px; }
    .preview-table th { padding: 8px 10px; background: var(--color-primary-300); color: white; text-align: left; font-size: 12px; }
    .preview-table td { padding: 7px 10px; border-bottom: 1px solid var(--color-border); }
    .text-sm { font-size: 12px; }
    .text-muted { color: var(--color-muted); }
    .btn--secondary { background: var(--color-surface-2); border: 1px solid var(--color-border); color: var(--color-text); padding: 8px 16px; border-radius: var(--radius-sm); font-size: 14px; cursor: pointer; font-weight: 500; transition: all .15s; &:hover:not(:disabled) { background: var(--color-border); } &:disabled { opacity: .5; cursor: not-allowed; } }
  `]
})
export class RelatorioEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(RelatoriosBIService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly StatusSessao = StatusSessao;
  readonly StatusLancamento = StatusLancamento;
  readonly TipoLancamento = TipoLancamento;

  editando = signal(false);
  salvando = signal(false);
  testando = signal(false);
  previewResultado = signal<RelatorioResultadoDto | null>(null);

  private relatorioId: string | null = null;

  form: FormGroup = this.fb.group({
    nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    descricao: [null],
    tipo: [null, Validators.required],
    agrupamento: [null],
    ordenacao: [null],
  });

  filtrosGroup: FormGroup = this.fb.group({
    dataInicio: [null],
    dataFim: [null],
    competencia: [null],
    psicologoId: [null],
    pacienteId: [null],
    statusSessao: [null],
    statusLancamento: [null],
    tipoLancamento: [null],
    planoContaId: [null],
  });

  readonly tiposOpcoes = [
    { valor: TipoRelatorio.Financeiro, label: 'Financeiro' },
    { valor: TipoRelatorio.Sessoes, label: 'Sessões' },
    { valor: TipoRelatorio.Pacientes, label: 'Pacientes' },
    { valor: TipoRelatorio.Psicologos, label: 'Psicólogos' },
    { valor: TipoRelatorio.Inadimplencia, label: 'Inadimplência' },
    { valor: TipoRelatorio.Repasses, label: 'Repasses' },
    { valor: TipoRelatorio.FluxoCaixaProjetado, label: 'Fluxo de Caixa Projetado' },
    { valor: TipoRelatorio.Comparativo, label: 'Comparativo Mensal' },
  ];

  ngOnInit(): void {
    this.relatorioId = this.route.snapshot.paramMap.get('id');
    this.editando.set(!!this.relatorioId);

    const navState = history.state as { template?: RelatorioTemplateDto };
    if (navState?.template) {
      this.aplicarTemplate(navState.template);
    }

    if (this.relatorioId) {
      this.carregarRelatorio(this.relatorioId);
    }
  }

  private carregarRelatorio(id: string): void {
    this.service
      .obterPorId(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((rel) => {
        this.form.patchValue({
          nome: rel.nome,
          descricao: rel.descricao,
          tipo: rel.tipo,
          agrupamento: rel.agrupamento,
          ordenacao: rel.ordenacao,
        });
        try {
          const filtros = JSON.parse(rel.filtrosJson) as RelatorioFiltrosDto;
          this.filtrosGroup.patchValue(filtros);
        } catch {
          // filtros inválidos — manter padrão
        }
      });
  }

  private aplicarTemplate(tpl: RelatorioTemplateDto): void {
    this.form.patchValue({
      nome: tpl.nome,
      descricao: tpl.descricao,
      tipo: tpl.tipo,
      agrupamento: tpl.agrupamento,
    });
    this.filtrosGroup.patchValue(tpl.filtrosPadrao);
  }

  exibirFiltroPsicologo(): boolean {
    const tipo = this.form.get('tipo')?.value as TipoRelatorio;
    return [TipoRelatorio.Sessoes, TipoRelatorio.Psicologos, TipoRelatorio.Repasses].includes(tipo);
  }

  exibirFiltroPaciente(): boolean {
    const tipo = this.form.get('tipo')?.value as TipoRelatorio;
    return [TipoRelatorio.Sessoes, TipoRelatorio.Pacientes, TipoRelatorio.Inadimplencia].includes(tipo);
  }

  exibirFiltroStatusSessao(): boolean {
    const tipo = this.form.get('tipo')?.value as TipoRelatorio;
    return tipo === TipoRelatorio.Sessoes;
  }

  exibirFiltroLancamento(): boolean {
    const tipo = this.form.get('tipo')?.value as TipoRelatorio;
    return [TipoRelatorio.Financeiro, TipoRelatorio.Pacientes, TipoRelatorio.Inadimplencia].includes(tipo);
  }

  testarFiltros(): void {
    if (this.form.get('tipo')?.invalid) return;

    this.testando.set(true);
    const request = this.montarRequestExecucao();

    this.service
      .executarAdHoc(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (resultado) => {
          this.previewResultado.set(resultado);
          this.testando.set(false);
        },
        error: () => this.testando.set(false),
      });
  }

  salvar(): void {
    if (this.form.invalid) return;

    this.salvando.set(true);
    const filtros = this.filtrosGroup.value as RelatorioFiltrosDto;

    if (this.editando() && this.relatorioId) {
      this.service
        .atualizar(this.relatorioId, {
          nome: this.form.value.nome,
          descricao: this.form.value.descricao,
          filtros,
          agrupamento: this.form.value.agrupamento,
          ordenacao: this.form.value.ordenacao,
        })
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (rel) => {
            this.salvando.set(false);
            this.router.navigate(['/relatorios-bi', rel.id, 'visualizar']);
          },
          error: () => this.salvando.set(false),
        });
    } else {
      const command: CriarRelatorioCommand = {
        nome: this.form.value.nome,
        descricao: this.form.value.descricao,
        tipo: this.form.value.tipo,
        filtros,
        agrupamento: this.form.value.agrupamento,
        ordenacao: this.form.value.ordenacao,
      };

      this.service
        .criar(command)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (rel) => {
            this.salvando.set(false);
            this.router.navigate(['/relatorios-bi', rel.id, 'visualizar']);
          },
          error: () => this.salvando.set(false),
        });
    }
  }

  private montarRequestExecucao(): ExecutarRelatorioRequest {
    return {
      tipo: this.form.value.tipo,
      filtros: this.filtrosGroup.value as RelatorioFiltrosDto,
      agrupamento: this.form.value.agrupamento,
      ordenacao: this.form.value.ordenacao,
    };
  }
}
