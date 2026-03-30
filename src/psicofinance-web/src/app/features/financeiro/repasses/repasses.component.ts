import {
  Component, ChangeDetectionStrategy, inject, signal, OnInit, DestroyRef
} from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../../core/services/api.service';

interface Repasse {
  id: string;
  psicologoId: string;
  psicologoNome: string;
  mesReferencia: string;
  valorCalculado: number;
  totalSessoes: number;
  status: 'Pendente' | 'Pago' | 'Cancelado';
  dataPagamento: string | null;
  observacao: string | null;
}

@Component({
  selector: 'app-repasses',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <div>
        <h2 class="heading-lg">Repasses para Psicólogos</h2>
        <p class="body-text" style="color: var(--color-muted); margin-top: 4px;">
          Gestão de repasses mensais para psicólogos PJ
        </p>
      </div>
      <button class="btn btn--primary" (click)="abrirGerarModal()">
        Gerar Repasses
      </button>
    </div>

    <div class="card">
      <div class="toolbar">
        <input class="input" type="month" [(ngModel)]="mesFiltro" (change)="carregar()" style="max-width: 180px;" />
        <select class="input" style="max-width: 160px;" [(ngModel)]="statusFiltro" (change)="carregar()">
          <option value="">Todos os status</option>
          <option value="Pendente">Pendente</option>
          <option value="Pago">Pago</option>
          <option value="Cancelado">Cancelado</option>
        </select>
      </div>

      @if (loading()) {
        <div class="loading-state"><span class="spinner-md"></span></div>
      } @else if (repasses().length === 0) {
        <div class="empty-state">
          <p>Nenhum repasse encontrado para o período selecionado.</p>
        </div>
      } @else {
        <table class="table">
          <thead>
            <tr>
              <th>Psicólogo</th>
              <th>Mês</th>
              <th>Sessões</th>
              <th>Valor</th>
              <th>Status</th>
              <th>Dt. Pagamento</th>
              <th style="width: 100px;">Ações</th>
            </tr>
          </thead>
          <tbody>
            @for (r of repasses(); track r.id) {
              <tr>
                <td class="font-medium">{{ r.psicologoNome }}</td>
                <td>{{ r.mesReferencia }}</td>
                <td>{{ r.totalSessoes }}</td>
                <td style="color: var(--color-success); font-weight: 600;">
                  {{ r.valorCalculado | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
                </td>
                <td>
                  <span class="badge" [ngClass]="{
                    'badge--neutral': r.status === 'Pendente',
                    'badge--success': r.status === 'Pago',
                    'badge--danger': r.status === 'Cancelado'
                  }">{{ r.status }}</span>
                </td>
                <td>{{ r.dataPagamento ?? '—' }}</td>
                <td>
                  @if (r.status === 'Pendente') {
                    <button class="btn btn--ghost btn--sm" (click)="pagar(r)">Pagar</button>
                  }
                </td>
              </tr>
            }
          </tbody>
          <tfoot>
            <tr>
              <td colspan="3" style="font-weight: 600;">Total</td>
              <td style="font-weight: 600; color: var(--color-success);">
                {{ totalRepasses() | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
              </td>
              <td colspan="3"></td>
            </tr>
          </tfoot>
        </table>
      }
    </div>

    <!-- Modal gerar repasses -->
    @if (gerarModalAberto()) {
      <div class="modal-overlay" (click)="fecharGerarModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>Gerar Repasses Mensais</h3>
            <button class="btn btn--ghost btn--icon" (click)="fecharGerarModal()">✕</button>
          </div>
          <div class="modal-body">
            @if (erro()) { <div class="alert alert--danger">{{ erro() }}</div> }
            <div class="form-group">
              <label class="form-label">Mês de Referência *</label>
              <input class="input" type="month" [(ngModel)]="gerarForm.mesReferencia" />
            </div>
            <p class="body-text" style="color: var(--color-muted);">
              Serão gerados repasses para todos os psicólogos PJ ativos com sessões realizadas no período.
              Repasses já gerados para o mesmo mês serão ignorados.
            </p>
          </div>
          <div class="modal-footer">
            <button class="btn btn--ghost" (click)="fecharGerarModal()">Cancelar</button>
            <button class="btn btn--primary" [disabled]="gerando()" (click)="gerar()">
              {{ gerando() ? 'Gerando...' : 'Gerar Repasses' }}
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class RepassesComponent implements OnInit {
  private api = inject(ApiService);
  private destroyRef = inject(DestroyRef);

  repasses = signal<Repasse[]>([]);
  loading = signal(true);
  gerarModalAberto = signal(false);
  gerando = signal(false);
  erro = signal<string | null>(null);
  totalRepasses = signal(0);

  mesFiltro = new Date().toISOString().slice(0, 7);
  statusFiltro = '';

  gerarForm = { mesReferencia: new Date().toISOString().slice(0, 7) };

  ngOnInit() { this.carregar(); }

  carregar() {
    this.loading.set(true);
    const params: Record<string, string> = {};
    if (this.mesFiltro) params['mesReferencia'] = this.mesFiltro;
    if (this.statusFiltro) params['status'] = this.statusFiltro;

    this.api.get<Repasse[]>('repasses', params)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.repasses.set(data);
          this.totalRepasses.set(data.filter(r => r.status !== 'Cancelado').reduce((s, r) => s + r.valorCalculado, 0));
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }

  abrirGerarModal() {
    this.gerarForm.mesReferencia = this.mesFiltro;
    this.erro.set(null);
    this.gerarModalAberto.set(true);
  }

  fecharGerarModal() { this.gerarModalAberto.set(false); }

  gerar() {
    this.gerando.set(true);
    this.erro.set(null);

    this.api.post<Repasse[]>('repasses/gerar', { mesReferencia: this.gerarForm.mesReferencia })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (novos) => {
          this.gerando.set(false);
          this.fecharGerarModal();
          this.mesFiltro = this.gerarForm.mesReferencia;
          this.carregar();
          if (novos.length === 0) alert('Nenhum repasse novo gerado. Verifique se há sessões realizadas no período.');
        },
        error: (e) => { this.gerando.set(false); this.erro.set(e?.error?.detail ?? 'Erro ao gerar repasses.'); }
      });
  }

  pagar(repasse: Repasse) {
    const data = prompt('Data de pagamento (YYYY-MM-DD):') || new Date().toISOString().slice(0, 10);
    this.api.patch(`repasses/${repasse.id}/pagar`, { dataPagamento: data, observacao: null })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.carregar());
  }
}
