import { Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../../core/services/api.service';

interface PsicologoDto {
  id: string;
  nome: string;
  crp: string;
  email: string | null;
  telefone: string | null;
  cpf: string | null;
  tipo: number;
  tipoRepasse: number;
  valorRepasse: number;
  banco: string | null;
  agencia: string | null;
  conta: string | null;
  pixChave: string | null;
  ativo: boolean;
}

interface PsicologoForm {
  nome: string;
  crp: string;
  email: string | null;
  telefone: string | null;
  cpf: string | null;
  tipo: number;
  tipoRepasse: number;
  valorRepasse: number;
  banco: string | null;
  agencia: string | null;
  conta: string | null;
  pixChave: string | null;
}

@Component({
  selector: 'app-psicologo-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <h2 class="heading-lg">{{ editando() ? 'Editar' : 'Novo' }} Psicólogo</h2>
    </div>

    @if (successMsg()) {
      <div class="alert alert--success animate-fade-up">{{ successMsg() }}</div>
    }
    @if (errorMsg()) {
      <div class="alert alert--error animate-fade-up">{{ errorMsg() }}</div>
    }

    <form class="config-form" (ngSubmit)="salvar()">
      <div class="card">
        <h3 class="heading-md section-title">Dados Pessoais</h3>
        <div class="form-grid">
          <div class="form-group form-group--2">
            <label class="label-text">Nome *</label>
            <input class="input" [(ngModel)]="form.nome" name="nome" required />
          </div>
          <div class="form-group">
            <label class="label-text">CRP *</label>
            <input class="input" [(ngModel)]="form.crp" name="crp" required placeholder="06/12345" />
          </div>
          <div class="form-group">
            <label class="label-text">Email</label>
            <input class="input" type="email" [(ngModel)]="form.email" name="email" />
          </div>
          <div class="form-group">
            <label class="label-text">Telefone</label>
            <input class="input" type="tel" [(ngModel)]="form.telefone" name="telefone" placeholder="(00) 00000-0000" />
          </div>
          <div class="form-group">
            <label class="label-text">CPF</label>
            <input class="input" [(ngModel)]="form.cpf" name="cpf" placeholder="000.000.000-00" />
          </div>
        </div>
      </div>

      <div class="card">
        <h3 class="heading-md section-title">Contrato e Repasse</h3>
        <div class="form-grid">
          <div class="form-group">
            <label class="label-text">Tipo *</label>
            <select class="input" [(ngModel)]="form.tipo" name="tipo">
              <option [ngValue]="1">PJ</option>
              <option [ngValue]="0">CLT</option>
            </select>
          </div>
          <div class="form-group">
            <label class="label-text">Tipo Repasse *</label>
            <select class="input" [(ngModel)]="form.tipoRepasse" name="tipoRepasse">
              <option [ngValue]="0">Percentual (%)</option>
              <option [ngValue]="1">Valor Fixo (R$)</option>
            </select>
          </div>
          <div class="form-group">
            <label class="label-text">Valor Repasse *</label>
            <input class="input" type="number" step="0.01" min="0" [(ngModel)]="form.valorRepasse" name="valorRepasse" />
          </div>
        </div>
      </div>

      <div class="card">
        <h3 class="heading-md section-title">Dados Bancários</h3>
        <div class="form-grid">
          <div class="form-group">
            <label class="label-text">Banco</label>
            <input class="input" [(ngModel)]="form.banco" name="banco" />
          </div>
          <div class="form-group">
            <label class="label-text">Agência</label>
            <input class="input" [(ngModel)]="form.agencia" name="agencia" />
          </div>
          <div class="form-group">
            <label class="label-text">Conta</label>
            <input class="input" [(ngModel)]="form.conta" name="conta" />
          </div>
          <div class="form-group form-group--full">
            <label class="label-text">Chave PIX</label>
            <input class="input" [(ngModel)]="form.pixChave" name="pixChave" />
          </div>
        </div>
      </div>

      <div class="form-actions">
        <button type="button" class="btn btn--secondary" (click)="voltar()">Cancelar</button>
        <button type="submit" class="btn btn--primary" [disabled]="loading() || !form.nome || !form.crp">
          @if (loading()) { <span class="spinner"></span> Salvando... } @else { Salvar }
        </button>
      </div>
    </form>
  `,
  styles: [`
    :host { display: block; }
    .page-header { margin-bottom: 20px; }
    .config-form { display: flex; flex-direction: column; gap: 16px; }
    .section-title { color: var(--color-text); margin-bottom: 16px; padding-bottom: 12px; border-bottom: 1px solid var(--color-border); }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 14px; }
    .form-group { display: flex; flex-direction: column; gap: 4px; }
    .form-group label { color: var(--color-muted); }
    .form-group--full { grid-column: 1 / -1; }
    .form-group--2 { grid-column: span 2; }
    .form-actions { display: flex; justify-content: flex-end; gap: 10px; padding-top: 8px; }
    .alert { padding: 10px 14px; border-radius: var(--radius-lg); font-size: 13px; margin-bottom: 16px; }
    .alert--success { background: var(--color-success-bg, #f0fdf4); color: #196040; border: 1px solid #bbf7d0; }
    .alert--error { background: #fef2f2; color: #dc2626; border: 1px solid #fecaca; }
    .spinner { width: 14px; height: 14px; border: 2px solid rgba(255,255,255,.3); border-top-color: #fff; border-radius: 50%; animation: spin .6s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    @media (max-width: 768px) { .form-grid { grid-template-columns: 1fr; } .form-group--2 { grid-column: span 1; } }
  `],
})
export class PsicologoFormComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  loading = signal(false);
  editando = signal(false);
  successMsg = signal('');
  errorMsg = signal('');
  private id: string | null = null;

  form: PsicologoForm = {
    nome: '', crp: '', email: null, telefone: null, cpf: null,
    tipo: 1, tipoRepasse: 0, valorRepasse: 0,
    banco: null, agencia: null, conta: null, pixChave: null,
  };

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    if (this.id) {
      this.editando.set(true);
      this.carregarPsicologo(this.id);
    }
  }

  private carregarPsicologo(id: string): void {
    this.loading.set(true);
    this.api.get<PsicologoDto>(`psicologos/${id}`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.form = {
            nome: data.nome, crp: data.crp, email: data.email,
            telefone: data.telefone, cpf: data.cpf, tipo: data.tipo,
            tipoRepasse: data.tipoRepasse, valorRepasse: data.valorRepasse,
            banco: data.banco, agencia: data.agencia, conta: data.conta, pixChave: data.pixChave,
          };
          this.loading.set(false);
        },
        error: () => { this.loading.set(false); this.errorMsg.set('Erro ao carregar psicólogo.'); },
      });
  }

  salvar(): void {
    this.loading.set(true);
    this.successMsg.set(''); this.errorMsg.set('');

    const req$ = this.editando()
      ? this.api.put<PsicologoDto>(`psicologos/${this.id}`, this.form)
      : this.api.post<PsicologoDto>('psicologos', this.form);

    req$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.loading.set(false);
        this.successMsg.set(this.editando() ? 'Psicólogo atualizado!' : 'Psicólogo cadastrado!');
        if (!this.editando()) {
          this.id = result.id;
          this.editando.set(true);
        }
        setTimeout(() => this.successMsg.set(''), 3000);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err.error?.detail || err.error?.title || 'Erro ao salvar.');
      },
    });
  }

  voltar(): void { this.router.navigate(['/cadastros/psicologos']); }
}
