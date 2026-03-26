import { Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../../core/services/api.service';

interface PacienteResumo {
  id: string;
  nome: string;
  cpf: string | null;
  telefone: string | null;
  ativo: boolean;
}

@Component({
  selector: 'app-pacientes-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Pacientes</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Gerencie os pacientes da clínica
        </p>
      </div>
      <button class="btn btn--primary" (click)="novo()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
        </svg>
        Novo Paciente
      </button>
    </div>

    <div class="card">
      <div class="toolbar">
        <input class="input" style="max-width: 300px;" type="text" placeholder="Buscar por nome ou CPF..."
               [(ngModel)]="busca" (input)="carregar()" />
        <label class="toggle-label">
          <input type="checkbox" [(ngModel)]="apenasAtivos" (change)="carregar()" />
          <span class="body-text">Apenas ativos</span>
        </label>
      </div>

      @if (loading()) {
        <div class="loading-state"><span class="spinner-md"></span></div>
      } @else if (pacientes().length === 0) {
        <div class="empty-state">
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="var(--color-hint)" stroke-width="1.5">
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
            <circle cx="12" cy="7" r="4"/>
          </svg>
          <p class="body-text" style="color: var(--color-hint);">Nenhum paciente encontrado</p>
        </div>
      } @else {
        <table class="table">
          <thead>
            <tr>
              <th>Nome</th>
              <th>CPF</th>
              <th>Telefone</th>
              <th>Status</th>
              <th style="width: 80px;"></th>
            </tr>
          </thead>
          <tbody>
            @for (p of pacientes(); track p.id) {
              <tr class="table__row--clickable" (click)="editar(p.id)">
                <td class="body-text">{{ p.nome }}</td>
                <td class="caption-text">{{ p.cpf || '—' }}</td>
                <td class="caption-text">{{ p.telefone || '—' }}</td>
                <td><span class="pill" [class]="p.ativo ? 'pill--realizada' : 'pill--cancelada'">{{ p.ativo ? 'Ativo' : 'Inativo' }}</span></td>
                <td>
                  <button class="btn btn--ghost btn--sm" (click)="editar(p.id); $event.stopPropagation()">
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
    .toggle-label { display: flex; align-items: center; gap: 6px; cursor: pointer; white-space: nowrap; }
    .toggle-label input { accent-color: var(--color-primary-300); }
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
export class PacientesListComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  pacientes = signal<PacienteResumo[]>([]);
  loading = signal(false);
  busca = '';
  apenasAtivos = true;

  ngOnInit(): void { this.carregar(); }

  carregar(): void {
    this.loading.set(true);
    const params = new URLSearchParams();
    if (this.busca) params.set('busca', this.busca);
    params.set('apenasAtivos', String(this.apenasAtivos));
    const qs = params.toString();

    this.api.get<PacienteResumo[]>(`pacientes${qs ? '?' + qs : ''}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.pacientes.set(data); this.loading.set(false); },
        error: () => this.loading.set(false),
      });
  }

  novo(): void { this.router.navigate(['/cadastros/pacientes/novo']); }
  editar(id: string): void { this.router.navigate(['/cadastros/pacientes', id]); }
}
