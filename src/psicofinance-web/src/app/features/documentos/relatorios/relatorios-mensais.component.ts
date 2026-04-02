import {
  Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  DocumentosService,
  RelatorioMensalDto,
  PsicologoResumo
} from '../documentos.service';

@Component({
  selector: 'app-relatorios-mensais',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Relatórios Mensais</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Geração de relatórios mensais por psicólogo
        </p>
      </div>
    </div>

    <!-- Card informativo -->
    <div class="card" style="margin-bottom: 16px; border-left: 4px solid var(--color-primary-300);">
      <div style="display: flex; gap: 12px; align-items: flex-start;">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="var(--color-primary-300)" stroke-width="2" stroke-linecap="round" style="flex-shrink: 0; margin-top: 1px;">
          <line x1="18" y1="20" x2="18" y2="10"/>
          <line x1="12" y1="20" x2="12" y2="4"/>
          <line x1="6" y1="20" x2="6" y2="14"/>
        </svg>
        <div>
          <p class="font-medium" style="color: var(--color-text);">Relatório Mensal de Desempenho</p>
          <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
            Inclui: sessões realizadas, faltas, cancelamentos, receita bruta do período
            e cálculo do repasse para o psicólogo. O PDF é gerado automaticamente e
            fica disponível para download.
          </p>
        </div>
      </div>
    </div>

    <div style="display: grid; grid-template-columns: 340px 1fr; gap: 16px; align-items: flex-start;">

      <!-- Formulário de geração -->
      <div class="card">
        <h3 class="heading-md" style="margin-bottom: 16px;">Gerar Relatório</h3>

        @if (erroGerar()) {
          <div class="alert alert--danger" style="margin-bottom: 12px;">{{ erroGerar() }}</div>
        }
        @if (sucesso()) {
          <div class="alert alert--success" style="margin-bottom: 12px;">Relatório gerado com sucesso!</div>
        }

        <div class="form-group">
          <label class="form-label">Psicólogo *</label>
          @if (loadingPsicologos()) {
            <div style="display: flex; align-items: center; gap: 8px; padding: 8px 0;">
              <span class="spinner-sm"></span>
              <span class="body-text" style="color: var(--color-muted);">Carregando...</span>
            </div>
          } @else {
            <select class="input" [(ngModel)]="form.psicologoId">
              <option value="">Selecione um psicólogo...</option>
              @for (p of psicologos(); track p.id) {
                <option [value]="p.id">{{ p.nome }}</option>
              }
            </select>
          }
        </div>

        <div class="form-group">
          <label class="form-label">Competência *</label>
          <input
            class="input"
            type="month"
            [(ngModel)]="form.competencia"
          />
          <span class="body-text" style="color: var(--color-muted); font-size: 12px; margin-top: 4px; display: block;">
            Selecione o mês e ano de referência.
          </span>
        </div>

        <button
          class="btn btn--primary"
          style="width: 100%;"
          [disabled]="gerando() || !formValido()"
          (click)="gerarRelatorio()"
        >
          @if (gerando()) {
            <span class="spinner-sm" style="margin-right: 8px;"></span>
            Gerando...
          } @else {
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="margin-right: 8px;">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
              <polyline points="14 2 14 8 20 8"/>
              <line x1="16" y1="13" x2="8" y2="13"/>
            </svg>
            Gerar Relatório
          }
        </button>
      </div>

      <!-- Lista de relatórios gerados na sessão -->
      <div>
        @if (relatoriosGerados().length === 0) {
          <div class="card">
            <div class="empty-state">
              <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="var(--color-hint)" stroke-width="1.5">
                <line x1="18" y1="20" x2="18" y2="10"/>
                <line x1="12" y1="20" x2="12" y2="4"/>
                <line x1="6" y1="20" x2="6" y2="14"/>
              </svg>
              <p style="margin-top: 12px;">Nenhum relatório gerado nesta sessão.</p>
              <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
                Use o formulário ao lado para gerar um relatório mensal.
              </p>
            </div>
          </div>
        } @else {
          <div style="display: flex; flex-direction: column; gap: 12px;">
            @for (relatorio of relatoriosGerados(); track relatorio.id) {
              <div class="card">
                <div style="display: flex; align-items: flex-start; justify-content: space-between; gap: 12px;">
                  <div style="flex: 1;">
                    <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 8px;">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="var(--color-primary-300)" stroke-width="2">
                        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                        <polyline points="14 2 14 8 20 8"/>
                      </svg>
                      <span class="font-medium">{{ relatorio.psicologoNome }}</span>
                      <span class="badge badge--neutral">{{ relatorio.competencia | date:'MM/yyyy' }}</span>
                    </div>
                    <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px;">
                      <div>
                        <p class="body-text" style="color: var(--color-muted); font-size: 12px;">Sessões</p>
                        <p class="font-medium">{{ relatorio.totalSessoes }}</p>
                      </div>
                      <div>
                        <p class="body-text" style="color: var(--color-muted); font-size: 12px;">Receita Total</p>
                        <p class="font-medium" style="color: var(--color-success);">
                          {{ relatorio.receitaTotal | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
                        </p>
                      </div>
                      <div>
                        <p class="body-text" style="color: var(--color-muted); font-size: 12px;">Repasse</p>
                        <p class="font-medium" style="color: var(--color-primary-300);">
                          {{ relatorio.valorRepasse | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
                        </p>
                      </div>
                    </div>
                    <p class="body-text" style="color: var(--color-hint); font-size: 11px; margin-top: 8px;">
                      Gerado em {{ relatorio.geradoEm | date:'dd/MM/yyyy HH:mm' }}
                    </p>
                  </div>
                  <button
                    class="btn btn--ghost btn--sm"
                    (click)="baixarRelatorio(relatorio)"
                    title="Baixar PDF"
                  >
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
                      <polyline points="7 10 12 15 17 10"/>
                      <line x1="12" y1="15" x2="12" y2="3"/>
                    </svg>
                    PDF
                  </button>
                </div>
              </div>
            }
          </div>
        }
      </div>
    </div>
  `
})
export class RelatoriosMensaisComponent implements OnInit {
  private service = inject(DocumentosService);
  private destroyRef = inject(DestroyRef);

  psicologos = signal<PsicologoResumo[]>([]);
  relatoriosGerados = signal<RelatorioMensalDto[]>([]);
  loadingPsicologos = signal(true);
  gerando = signal(false);
  erroGerar = signal<string | null>(null);
  sucesso = signal(false);

  form = {
    psicologoId: '',
    competencia: new Date().toISOString().slice(0, 7)
  };

  ngOnInit() { this.carregarPsicologos(); }

  carregarPsicologos() {
    this.loadingPsicologos.set(true);
    this.service.getPsicologos()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.psicologos.set(data); this.loadingPsicologos.set(false); },
        error: () => this.loadingPsicologos.set(false)
      });
  }

  formValido(): boolean {
    return this.form.psicologoId.length > 0 && this.form.competencia.length === 7;
  }

  gerarRelatorio() {
    if (!this.formValido()) return;
    this.gerando.set(true);
    this.erroGerar.set(null);
    this.sucesso.set(false);

    this.service.gerarRelatorioMensal({
      psicologoId: this.form.psicologoId,
      competencia: this.form.competencia
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (relatorio) => {
          this.gerando.set(false);
          this.sucesso.set(true);
          this.relatoriosGerados.update(lista => [relatorio, ...lista]);
          setTimeout(() => this.sucesso.set(false), 3000);
        },
        error: (e: { error?: { detail?: string } }) => {
          this.gerando.set(false);
          this.erroGerar.set(e?.error?.detail ?? 'Erro ao gerar relatório.');
        }
      });
  }

  baixarRelatorio(relatorio: RelatorioMensalDto) {
    const filePath = encodeURIComponent(relatorio.arquivoUrl);
    this.service.downloadRelatorio(filePath)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `relatorio-${relatorio.psicologoNome.replace(/\s+/g, '-')}-${relatorio.competencia}.pdf`;
          link.click();
          URL.revokeObjectURL(url);
        },
        error: () => alert('Erro ao baixar o PDF do relatório.')
      });
  }
}
