import { Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../../core/services/api.service';

interface ContratoResumo {
  id: string;
  pacienteNome: string;
  psicologoNome: string;
  valorSessao: number;
  frequencia: string;
  diaSemanaSessao: string;
  horarioSessao: string;
  status: string;
}

@Component({
  selector: 'app-contratos-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Contratos</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Gerencie os contratos entre pacientes e psicólogos
        </p>
      </div>
      <button class="btn btn--primary" (click)="novo()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
        </svg>
        Novo Contrato
      </button>
    </div>

    <div class="card">
      <div class="toolbar">
        <input class="input" style="max-width: 300px;" type="text" placeholder="Buscar por paciente ou psicólogo..."
               [(ngModel)]="busca" (input)="carregar()" />
        <select class="input" style="max-width: 180px;" [(ngModel)]="statusFiltro" (change)="carregar()">
          <option value="">Todos os status</option>
          <option value="Ativo">Ativo</option>
          <option value="Pausado">Pausado</option>
          <option value="Encerrado">Encerrado</option>
        </select>
      </div>

      @if (loading()) {
        <div class="loading-state"><span class="spinner-md"></span></div>
      } @else if (contratos().length === 0) {
        <div class="empty-state">
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="var(--color-hint)" stroke-width="1.5">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
            <polyline points="14 2 14 8 20 8"/>
            <line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/>
            <polyline points="10 9 9 9 8 9"/>
          </svg>
          <p class="body-text" style="color: var(--color-hint);">Nenhum contrato encontrado</p>
        </div>
      } @else {
        <table class="table">
          <thead>
            <tr>
              <th>Paciente</th>
              <th>Psicólogo</th>
              <th>Valor</th>
              <th>Frequência</th>
              <th>Dia/Horário</th>
              <th>Status</th>
              <th style="width: 80px;"></th>
            </tr>
          </thead>
          <tbody>
            @for (c of contratos(); track c.id) {
              <tr class="table__row--clickable" (click)="editar(c.id)">
                <td class="body-text">{{ c.pacienteNome }}</td>
                <td class="body-text">{{ c.psicologoNome }}</td>
                <td class="caption-text">{{ c.valorSessao | currency:'BRL' }}</td>
                <td class="caption-text">{{ traduzirFrequencia(c.frequencia) }}</td>
                <td class="caption-text">{{ traduzirDia(c.diaSemanaSessao) }} {{ c.horarioSessao | slice:0:5 }}</td>
                <td>
                  <span class="pill" [class]="statusClass(c.status)">{{ c.status }}</span>
                </td>
                <td>
                  <button class="btn btn--ghost btn--sm" (click)="editar(c.id); $event.stopPropagation()">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                      <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
                    </svg>
                  </button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
    .toolbar { display: flex; align-items: center; gap: 16px; margin-bottom: 16px; }
    .table { width: 100%; border-collapse: collapse; }
    .table th { text-align: left; padding: 8px 12px; font-size: 11px; font-weight: 600; color: var(--color-hint); text-transform: uppercase; letter-spacing: 0.5px; border-bottom: 1px solid var(--color-border); }
    .table td { padding: 10px 12px; border-bottom: 1px solid var(--color-border); }
    .table__row--clickable { cursor: pointer; transition: background .15s; }
    .table__row--clickable:hover { background: var(--color-surface-2); }
    .empty-state { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 40px; }
    .loading-state { display: flex; justify-content: center; padding: 40px; }
    .spinner-md { width: 24px; height: 24px; border: 3px solid var(--color-border); border-top-color: var(--color-primary-300); border-radius: 50%; animation: spin .6s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
})
export class ContratosListComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  contratos = signal<ContratoResumo[]>([]);
  loading = signal(false);
  busca = '';
  statusFiltro = '';

  ngOnInit(): void { this.carregar(); }

  carregar(): void {
    this.loading.set(true);
    const params = new URLSearchParams();
    if (this.busca) params.set('busca', this.busca);
    if (this.statusFiltro) params.set('status', this.statusFiltro);
    const qs = params.toString();

    this.api.get<ContratoResumo[]>(`contratos${qs ? '?' + qs : ''}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.contratos.set(data); this.loading.set(false); },
        error: () => this.loading.set(false),
      });
  }

  novo(): void { this.router.navigate(['/cadastros/contratos/novo']); }
  editar(id: string): void { this.router.navigate(['/cadastros/contratos', id]); }

  statusClass(status: string): string {
    switch (status) {
      case 'Ativo': return 'pill pill--realizada';
      case 'Pausado': return 'pill pill--agendada';
      case 'Encerrado': return 'pill pill--cancelada';
      default: return 'pill';
    }
  }

  traduzirFrequencia(freq: string): string {
    return freq === 'Semanal' ? 'Semanal' : 'Quinzenal';
  }

  traduzirDia(dia: string): string {
    const map: Record<string, string> = {
      'Segunda': 'Seg', 'Terca': 'Ter', 'Quarta': 'Qua',
      'Quinta': 'Qui', 'Sexta': 'Sex', 'Sabado': 'Sáb', 'Domingo': 'Dom'
    };
    return map[dia] || dia;
  }
}
