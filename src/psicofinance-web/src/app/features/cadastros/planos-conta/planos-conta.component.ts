import {
  Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../../core/services/api.service';

interface PlanoConta {
  id: string;
  nome: string;
  tipo: 'Receita' | 'Despesa';
  descricao: string | null;
  ativo: boolean;
}

@Component({
  selector: 'app-planos-conta',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Planos de Conta</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Categorias para classificar receitas e despesas
        </p>
      </div>
      <button class="btn btn--primary" (click)="abrirFormulario()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
        </svg>
        Novo Plano
      </button>
    </div>

    <div class="card">
      <div class="toolbar">
        <select class="input" style="max-width: 180px;" [(ngModel)]="tipoFiltro" (change)="carregar()">
          <option value="">Todos os tipos</option>
          <option value="Receita">Receita</option>
          <option value="Despesa">Despesa</option>
        </select>
        <select class="input" style="max-width: 160px;" [(ngModel)]="ativoFiltro" (change)="carregar()">
          <option value="">Todos</option>
          <option value="true">Ativos</option>
          <option value="false">Inativos</option>
        </select>
      </div>

      @if (loading()) {
        <div class="loading-state"><span class="spinner-md"></span></div>
      } @else if (planos().length === 0) {
        <div class="empty-state">
          <p>Nenhum plano de conta encontrado.</p>
        </div>
      } @else {
        <table class="table">
          <thead>
            <tr>
              <th>Nome</th>
              <th>Tipo</th>
              <th>Descrição</th>
              <th>Status</th>
              <th style="width: 100px;">Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (plano of planos(); track plano.id) {
              <tr>
                <td class="font-medium">{{ plano.nome }}</td>
                <td>
                  <span class="badge" [class]="plano.tipo === 'Receita' ? 'badge--success' : 'badge--danger'">
                    {{ plano.tipo }}
                  </span>
                </td>
                <td class="text-muted">{{ plano.descricao || '—' }}</td>
                <td>
                  <span class="badge" [class]="plano.ativo ? 'badge--success' : 'badge--neutral'">
                    {{ plano.ativo ? 'Ativo' : 'Inativo' }}
                  </span>
                </td>
                <td>
                  <button class="btn btn--ghost btn--sm" (click)="editar(plano)">Editar</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>

    <!-- Modal de formulário -->
    @if (modalAberto()) {
      <div class="modal-overlay" (click)="fecharModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>{{ editandoId() ? 'Editar Plano' : 'Novo Plano de Conta' }}</h3>
            <button class="btn btn--ghost btn--icon" (click)="fecharModal()">✕</button>
          </div>
          <div class="modal-body">
            @if (erro()) {
              <div class="alert alert--danger">{{ erro() }}</div>
            }
            <div class="form-group">
              <label class="form-label">Nome *</label>
              <input class="input" [(ngModel)]="form.nome" placeholder="Ex: Sessões de Terapia" />
            </div>
            <div class="form-group">
              <label class="form-label">Tipo *</label>
              <select class="input" [(ngModel)]="form.tipo">
                <option value="Receita">Receita</option>
                <option value="Despesa">Despesa</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Descrição</label>
              <input class="input" [(ngModel)]="form.descricao" placeholder="Opcional" />
            </div>
            @if (editandoId()) {
              <div class="form-group">
                <label class="form-label">Status</label>
                <select class="input" [(ngModel)]="form.ativo">
                  <option [ngValue]="true">Ativo</option>
                  <option [ngValue]="false">Inativo</option>
                </select>
              </div>
            }
          </div>
          <div class="modal-footer">
            <button class="btn btn--ghost" (click)="fecharModal()">Cancelar</button>
            <button class="btn btn--primary" [disabled]="salvando() || !form.nome"
                    (click)="salvar()">
              {{ salvando() ? 'Salvando...' : 'Salvar' }}
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class PlanosContaComponent implements OnInit {
  private api = inject(ApiService);
  private destroyRef = inject(DestroyRef);

  planos = signal<PlanoConta[]>([]);
  loading = signal(true);
  modalAberto = signal(false);
  salvando = signal(false);
  editandoId = signal<string | null>(null);
  erro = signal<string | null>(null);

  tipoFiltro = '';
  ativoFiltro = '';

  form: { nome: string; tipo: string; descricao: string; ativo: boolean } = {
    nome: '', tipo: 'Receita', descricao: '', ativo: true
  };

  ngOnInit() { this.carregar(); }

  carregar() {
    this.loading.set(true);
    const params: Record<string, string> = {};
    if (this.tipoFiltro) params['tipo'] = this.tipoFiltro;
    if (this.ativoFiltro !== '') params['ativo'] = this.ativoFiltro;

    this.api.get<PlanoConta[]>('planos-conta', params)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => { this.planos.set(data); this.loading.set(false); },
        error: () => this.loading.set(false)
      });
  }

  abrirFormulario() {
    this.editandoId.set(null);
    this.form = { nome: '', tipo: 'Receita', descricao: '', ativo: true };
    this.erro.set(null);
    this.modalAberto.set(true);
  }

  editar(plano: PlanoConta) {
    this.editandoId.set(plano.id);
    this.form = { nome: plano.nome, tipo: plano.tipo, descricao: plano.descricao ?? '', ativo: plano.ativo };
    this.erro.set(null);
    this.modalAberto.set(true);
  }

  fecharModal() { this.modalAberto.set(false); }

  salvar() {
    if (!this.form.nome.trim()) return;
    this.salvando.set(true);
    this.erro.set(null);

    const body = { nome: this.form.nome, tipo: this.form.tipo, descricao: this.form.descricao || null };
    const id = this.editandoId();
    const req = id
      ? this.api.put<PlanoConta>(`planos-conta/${id}`, { ...body, ativo: this.form.ativo })
      : this.api.post<PlanoConta>('planos-conta', body);

    req.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.salvando.set(false); this.fecharModal(); this.carregar(); },
      error: (e) => {
        this.salvando.set(false);
        this.erro.set(e?.error?.detail ?? 'Erro ao salvar. Tente novamente.');
      }
    });
  }
}
