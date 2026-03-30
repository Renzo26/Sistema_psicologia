import { Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
  selector: 'app-sessoes-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Sessões</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Gerencie as sessões e controle de frequência
        </p>
      </div>
      <div style="display:flex; gap:8px;">
        <button class="btn btn--secondary" (click)="irParaAgenda()">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/>
            <line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>
          </svg>
          Agenda
        </button>
        <button class="btn btn--primary" (click)="nova()">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
          </svg>
          Nova Sessão
        </button>
      </div>
    </div>

    <div class="card">
      <div class="toolbar">
        <input class="input" type="date" [(ngModel)]="dataInicio" (change)="carregar()" style="max-width:160px;" />
        <input class="input" type="date" [(ngModel)]="dataFim" (change)="carregar()" style="max-width:160px;" />
        <select class="input" style="max-width:160px;" [(ngModel)]="statusFiltro" (change)="carregar()">
          <option value="">Todos os status</option>
          <option value="Agendada">Agendada</option>
          <option value="Realizada">Realizada</option>
          <option value="Falta">Falta</option>
          <option value="FaltaJustificada">Falta Justificada</option>
          <option value="Cancelada">Cancelada</option>
        </select>
      </div>

      @if (loading()) {
        <div class="loading-state"><span class="spinner-md"></span></div>
      } @else if (sessoes().length === 0) {
        <div class="empty-state">
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="var(--color-hint)" stroke-width="1.5">
            <rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/>
            <line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>
          </svg>
          <p class="body-text" style="color: var(--color-hint);">Nenhuma sessão encontrada</p>
        </div>
      } @else {
        <table class="table">
          <thead>
            <tr>
              <th>Data</th>
              <th>Horário</th>
              <th>Paciente</th>
              <th>Psicólogo</th>
              <th>Status</th>
              <th>Presença</th>
              <th style="width:60px;"></th>
            </tr>
          </thead>
          <tbody>
            @for (s of sessoes(); track s.id) {
              <tr class="table__row--clickable" (click)="editar(s.id)">
                <td class="body-text">{{ formatarData(s.data) }}</td>
                <td class="caption-text">{{ s.horarioInicio | slice:0:5 }}</td>
                <td class="body-text">{{ s.pacienteNome }}</td>
                <td class="caption-text">{{ s.psicologoNome }}</td>
                <td>
                  <span class="pill" [class]="statusClass(s.status)">
                    {{ traduzirStatus(s.status) }}
                  </span>
                </td>
                <td (click)="$event.stopPropagation()">
                  @if (s.status === 'Agendada') {
                    <div class="presenca-actions">
                      <button class="btn-icon btn-icon--success" title="Marcar presença"
                              (click)="marcarPresenca(s.id)">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                          <polyline points="20 6 9 17 4 12"/>
                        </svg>
                      </button>
                      <button class="btn-icon btn-icon--danger" title="Registrar falta"
                              (click)="registrarFalta(s.id, false)">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                          <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                        </svg>
                      </button>
                      <button class="btn-icon btn-icon--warning" title="Falta justificada"
                              (click)="registrarFalta(s.id, true)">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                          <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/>
                          <line x1="12" y1="16" x2="12.01" y2="16"/>
                        </svg>
                      </button>
                    </div>
                  }
                </td>
                <td>
                  <button class="btn btn--ghost btn--sm" (click)="editar(s.id); $event.stopPropagation()">
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

    @if (toastMsg()) {
      <div class="toast" [class.toast--error]="toastErro()">{{ toastMsg() }}</div>
    }
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
    .toolbar { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; flex-wrap: wrap; }
    .table { width: 100%; border-collapse: collapse; }
    .table th { text-align: left; padding: 8px 12px; font-size: 11px; font-weight: 600; color: var(--color-hint); text-transform: uppercase; letter-spacing: 0.5px; border-bottom: 1px solid var(--color-border); }
    .table td { padding: 10px 12px; border-bottom: 1px solid var(--color-border); vertical-align: middle; }
    .table__row--clickable { cursor: pointer; transition: background .15s; }
    .table__row--clickable:hover { background: var(--color-surface-2); }
    .empty-state { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 40px; }
    .loading-state { display: flex; justify-content: center; padding: 40px; }
    .spinner-md { width: 24px; height: 24px; border: 3px solid var(--color-border); border-top-color: var(--color-primary-300); border-radius: 50%; animation: spin .6s linear infinite; }
    .presenca-actions { display: flex; gap: 4px; align-items: center; }
    .btn-icon { display: inline-flex; align-items: center; justify-content: center; width: 26px; height: 26px; border-radius: 6px; border: 1px solid; cursor: pointer; transition: all .15s; }
    .btn-icon--success { border-color: #86efac; color: #16a34a; background: #f0fdf4; }
    .btn-icon--success:hover { background: #dcfce7; }
    .btn-icon--danger { border-color: #fca5a5; color: #dc2626; background: #fef2f2; }
    .btn-icon--danger:hover { background: #fee2e2; }
    .btn-icon--warning { border-color: #fcd34d; color: #d97706; background: #fffbeb; }
    .btn-icon--warning:hover { background: #fef3c7; }
    .toast { position: fixed; bottom: 24px; right: 24px; background: #1a1a1a; color: #fff; padding: 12px 18px; border-radius: 8px; font-size: 13px; z-index: 1000; animation: fadeIn .3s; }
    .toast--error { background: #dc2626; }
    @keyframes spin { to { transform: rotate(360deg); } }
    @keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
  `],
})
export class SessoesListComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  sessoes = signal<SessaoResumo[]>([]);
  loading = signal(false);
  toastMsg = signal('');
  toastErro = signal(false);

  dataInicio = '';
  dataFim = '';
  statusFiltro = '';

  ngOnInit(): void {
    const hoje = new Date();
    const primeiroDia = new Date(hoje.getFullYear(), hoje.getMonth(), 1);
    const ultimoDia = new Date(hoje.getFullYear(), hoje.getMonth() + 1, 0);
    this.dataInicio = this.toDateInput(primeiroDia);
    this.dataFim = this.toDateInput(ultimoDia);
    this.carregar();
  }

  carregar(): void {
    this.loading.set(true);
    const params = new URLSearchParams();
    if (this.dataInicio) params.set('dataInicio', this.dataInicio);
    if (this.dataFim) params.set('dataFim', this.dataFim);
    if (this.statusFiltro) params.set('status', this.statusFiltro);
    const qs = params.toString();

    this.api.get<SessaoResumo[]>(`sessoes${qs ? '?' + qs : ''}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.sessoes.set(data); this.loading.set(false); },
        error: () => this.loading.set(false),
      });
  }

  marcarPresenca(id: string): void {
    this.api.patch<void>(`sessoes/${id}/presenca`, {})
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => { this.mostrarToast('Presença marcada!', false); this.carregar(); },
        error: (err) => this.mostrarToast(err.error?.detail || 'Erro ao marcar presença', true),
      });
  }

  registrarFalta(id: string, justificada: boolean): void {
    this.api.patch<void>(`sessoes/${id}/falta`, { justificada, motivo: null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => { this.mostrarToast(justificada ? 'Falta justificada registrada!' : 'Falta registrada!', false); this.carregar(); },
        error: (err) => this.mostrarToast(err.error?.detail || 'Erro ao registrar falta', true),
      });
  }

  nova(): void { this.router.navigate(['/sessoes/nova']); }
  editar(id: string): void { this.router.navigate(['/sessoes', id]); }
  irParaAgenda(): void { this.router.navigate(['/sessoes/agenda']); }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      'Agendada': 'pill pill--agendada',
      'Realizada': 'pill pill--realizada',
      'Falta': 'pill pill--falta',
      'FaltaJustificada': 'pill pill--falta-justificada',
      'Cancelada': 'pill pill--cancelada',
    };
    return map[status] ?? 'pill';
  }

  traduzirStatus(status: string): string {
    const map: Record<string, string> = {
      'Agendada': 'Agendada',
      'Realizada': 'Realizada',
      'Falta': 'Falta',
      'FaltaJustificada': 'Falta Justif.',
      'Cancelada': 'Cancelada',
    };
    return map[status] ?? status;
  }

  formatarData(data: string): string {
    if (!data) return '';
    const [y, m, d] = data.split('-');
    return `${d}/${m}/${y}`;
  }

  private mostrarToast(msg: string, erro: boolean): void {
    this.toastMsg.set(msg);
    this.toastErro.set(erro);
    setTimeout(() => this.toastMsg.set(''), 3000);
  }

  private toDateInput(d: Date): string {
    return d.toISOString().split('T')[0];
  }
}
