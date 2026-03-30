import { Component, ChangeDetectionStrategy, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../core/services/api.service';

interface SessaoResumo {
  id: string;
  contratoId: string;
  pacienteNome: string;
  psicologoNome: string;
  data: string;
  horarioInicio: string;
  duracaoMinutos: number;
  status: string;
}

@Component({
  selector: 'app-sessoes-agenda',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Agenda de Sessões</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Visualização semanal das sessões agendadas
        </p>
      </div>
      <button class="btn btn--secondary" (click)="irParaLista()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/>
          <line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/>
          <line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/>
        </svg>
        Listagem
      </button>
    </div>

    <div class="card">
      <div class="calendar-nav">
        <button class="btn btn--ghost btn--sm" (click)="semanaAnterior()">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="15 18 9 12 15 6"/>
          </svg>
        </button>
        <span class="heading-md" style="color: var(--color-text);">
          {{ formatarPeriodo() }}
        </span>
        <button class="btn btn--ghost btn--sm" (click)="proximaSemana()">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="9 18 15 12 9 6"/>
          </svg>
        </button>
        <button class="btn btn--ghost btn--sm" (click)="irParaHoje()" style="margin-left:8px;">Hoje</button>
      </div>

      @if (loading()) {
        <div class="loading-state"><span class="spinner-md"></span></div>
      } @else {
        <div class="calendar-grid">
          @for (dia of diasDaSemana(); track dia.data) {
            <div class="calendar-col" [class.calendar-col--hoje]="dia.isHoje">
              <div class="calendar-col__header">
                <span class="caption-text" style="color: var(--color-hint);">{{ dia.diaNome }}</span>
                <span class="heading-md" [style.color]="dia.isHoje ? 'var(--color-primary-300)' : 'var(--color-text)'">
                  {{ dia.diaNum }}
                </span>
              </div>
              <div class="calendar-col__body">
                @for (s of getSessoesDodia(dia.data); track s.id) {
                  <div class="sessao-card" [class]="'sessao-card--' + s.status.toLowerCase()"
                       (click)="editar(s.id)" title="{{ s.pacienteNome }} — {{ s.psicologoNome }}">
                    <span class="sessao-card__hora">{{ s.horarioInicio | slice:0:5 }}</span>
                    <span class="sessao-card__paciente">{{ s.pacienteNome }}</span>
                    <span class="sessao-card__psi">{{ s.psicologoNome }}</span>
                  </div>
                }
                @if (getSessoesDodia(dia.data).length === 0) {
                  <div class="calendar-col__empty"></div>
                }
              </div>
            </div>
          }
        </div>
      }

      <div class="legenda">
        <span class="legenda-item"><span class="dot dot--agendada"></span> Agendada</span>
        <span class="legenda-item"><span class="dot dot--realizada"></span> Realizada</span>
        <span class="legenda-item"><span class="dot dot--falta"></span> Falta</span>
        <span class="legenda-item"><span class="dot dot--faltajustificada"></span> Falta Just.</span>
        <span class="legenda-item"><span class="dot dot--cancelada"></span> Cancelada</span>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
    .calendar-nav { display: flex; align-items: center; gap: 8px; margin-bottom: 16px; padding-bottom: 12px; border-bottom: 1px solid var(--color-border); }
    .loading-state { display: flex; justify-content: center; padding: 40px; }
    .spinner-md { width: 24px; height: 24px; border: 3px solid var(--color-border); border-top-color: var(--color-primary-300); border-radius: 50%; animation: spin .6s linear infinite; }

    .calendar-grid { display: grid; grid-template-columns: repeat(7, 1fr); gap: 4px; min-height: 400px; }

    .calendar-col { display: flex; flex-direction: column; border-right: 1px solid var(--color-border); }
    .calendar-col:last-child { border-right: none; }
    .calendar-col--hoje .calendar-col__header { background: var(--color-primary-50); border-radius: 6px 6px 0 0; }

    .calendar-col__header { display: flex; flex-direction: column; align-items: center; padding: 8px 4px; gap: 2px; }
    .calendar-col__body { flex: 1; display: flex; flex-direction: column; gap: 4px; padding: 4px; min-height: 200px; }
    .calendar-col__empty { flex: 1; }

    .sessao-card { padding: 6px 8px; border-radius: 6px; cursor: pointer; transition: opacity .15s; display: flex; flex-direction: column; gap: 2px; }
    .sessao-card:hover { opacity: .85; }
    .sessao-card__hora { font-size: 10px; font-weight: 700; }
    .sessao-card__paciente { font-size: 11px; font-weight: 600; line-height: 1.2; }
    .sessao-card__psi { font-size: 10px; opacity: .75; }

    .sessao-card--agendada { background: #dbeafe; color: #1e40af; }
    .sessao-card--realizada { background: #dcfce7; color: #15803d; }
    .sessao-card--falta { background: #fee2e2; color: #dc2626; }
    .sessao-card--faltajustificada { background: #fef3c7; color: #92400e; }
    .sessao-card--cancelada { background: var(--color-surface-2); color: var(--color-hint); text-decoration: line-through; }

    .legenda { display: flex; gap: 16px; flex-wrap: wrap; padding-top: 16px; border-top: 1px solid var(--color-border); margin-top: 8px; }
    .legenda-item { display: flex; align-items: center; gap: 6px; font-size: 12px; color: var(--color-muted); }
    .dot { width: 10px; height: 10px; border-radius: 50%; }
    .dot--agendada { background: #3b82f6; }
    .dot--realizada { background: #16a34a; }
    .dot--falta { background: #dc2626; }
    .dot--faltajustificada { background: #d97706; }
    .dot--cancelada { background: var(--color-hint); }

    @keyframes spin { to { transform: rotate(360deg); } }
    @media (max-width: 768px) { .calendar-grid { grid-template-columns: repeat(3, 1fr); } }
  `],
})
export class SessoesAgendaComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  sessoes = signal<SessaoResumo[]>([]);
  loading = signal(false);
  private semanaBase = signal<Date>(this.inicioSemana(new Date()));

  readonly diasDaSemana = computed(() => {
    const inicio = this.semanaBase();
    const hoje = new Date();
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(inicio);
      d.setDate(inicio.getDate() + i);
      return {
        data: this.toDateStr(d),
        diaNome: d.toLocaleDateString('pt-BR', { weekday: 'short' }).replace('.', ''),
        diaNum: d.getDate(),
        isHoje: this.toDateStr(d) === this.toDateStr(hoje),
      };
    });
  });

  ngOnInit(): void { this.carregar(); }

  private carregar(): void {
    const dias = this.diasDaSemana();
    const inicio = dias[0].data;
    const fim = dias[6].data;
    this.loading.set(true);

    this.api.get<SessaoResumo[]>(`sessoes?dataInicio=${inicio}&dataFim=${fim}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.sessoes.set(data); this.loading.set(false); },
        error: () => this.loading.set(false),
      });
  }

  getSessoesDodia(data: string): SessaoResumo[] {
    return this.sessoes().filter(s => s.data === data)
      .sort((a, b) => a.horarioInicio.localeCompare(b.horarioInicio));
  }

  semanaAnterior(): void { this.moverSemana(-7); }
  proximaSemana(): void { this.moverSemana(7); }
  irParaHoje(): void { this.semanaBase.set(this.inicioSemana(new Date())); this.carregar(); }
  irParaLista(): void { this.router.navigate(['/sessoes']); }
  editar(id: string): void { this.router.navigate(['/sessoes', id]); }

  formatarPeriodo(): string {
    const dias = this.diasDaSemana();
    const ini = new Date(dias[0].data + 'T00:00:00');
    const fim = new Date(dias[6].data + 'T00:00:00');
    return `${ini.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' })} — ${fim.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' })}`;
  }

  private moverSemana(dias: number): void {
    const nova = new Date(this.semanaBase());
    nova.setDate(nova.getDate() + dias);
    this.semanaBase.set(nova);
    this.carregar();
  }

  private inicioSemana(data: Date): Date {
    const d = new Date(data);
    const dia = d.getDay(); // 0=Dom
    const diff = dia === 0 ? -6 : 1 - dia; // segunda
    d.setDate(d.getDate() + diff);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  private toDateStr(d: Date): string {
    return d.toISOString().split('T')[0];
  }
}
