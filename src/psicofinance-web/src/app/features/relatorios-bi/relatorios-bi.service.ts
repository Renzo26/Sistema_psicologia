import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// ── Enums ─────────────────────────────────────────────────────────

export enum TipoRelatorio {
  Financeiro = 0,
  Sessoes = 1,
  Pacientes = 2,
  Psicologos = 3,
  Inadimplencia = 4,
  Repasses = 5,
  FluxoCaixaProjetado = 6,
  Comparativo = 7
}

export enum FormatoExportacao {
  Json = 0,
  Pdf = 1,
  Xlsx = 2,
  Csv = 3
}

export enum StatusSessao {
  Agendada = 0,
  Realizada = 1,
  Falta = 2,
  FaltaJustificada = 3,
  Cancelada = 4
}

export enum StatusLancamento {
  Previsto = 0,
  Confirmado = 1,
  Cancelado = 2
}

export enum TipoLancamento {
  Receita = 0,
  Despesa = 1
}

// ── DTOs ──────────────────────────────────────────────────────────

export interface RelatorioFiltrosDto {
  dataInicio?: string | null;
  dataFim?: string | null;
  competencia?: string | null;
  psicologoId?: string | null;
  pacienteId?: string | null;
  statusSessao?: StatusSessao | null;
  statusLancamento?: StatusLancamento | null;
  tipoLancamento?: TipoLancamento | null;
  planoContaId?: string | null;
}

export interface RelatorioPersonalizadoDto {
  id: string;
  nome: string;
  descricao: string | null;
  tipo: TipoRelatorio;
  filtrosJson: string;
  agrupamento: string | null;
  ordenacao: string | null;
  favorito: boolean;
  criadoPorId: string;
  criadoEm: string;
  atualizadoEm: string;
}

export interface RelatorioResultadoDto {
  titulo: string;
  descricao: string | null;
  colunas: string[];
  linhas: Record<string, unknown>[];
  totalRegistros: number;
  agrupamento: string | null;
  geradoEm: string;
}

export interface RelatorioTemplateDto {
  id: string;
  nome: string;
  descricao: string;
  tipo: TipoRelatorio;
  filtrosPadrao: RelatorioFiltrosDto;
  agrupamento: string | null;
}

export interface CriarRelatorioCommand {
  nome: string;
  descricao?: string | null;
  tipo: TipoRelatorio;
  filtros: RelatorioFiltrosDto;
  agrupamento?: string | null;
  ordenacao?: string | null;
}

export interface AtualizarRelatorioRequest {
  nome: string;
  descricao?: string | null;
  filtros: RelatorioFiltrosDto;
  agrupamento?: string | null;
  ordenacao?: string | null;
}

export interface ExecutarRelatorioRequest {
  tipo: TipoRelatorio;
  filtros: RelatorioFiltrosDto;
  agrupamento?: string | null;
  ordenacao?: string | null;
}

export interface ExportarRelatorioRequest {
  tipo: TipoRelatorio;
  filtros: RelatorioFiltrosDto;
  formato: FormatoExportacao;
}

// ── Service ───────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class RelatoriosBIService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/relatorios-bi`;

  listar(tipo?: TipoRelatorio, apenasFavorito?: boolean): Observable<RelatorioPersonalizadoDto[]> {
    let params = new HttpParams();
    if (tipo !== undefined && tipo !== null) params = params.set('tipo', tipo.toString());
    if (apenasFavorito !== undefined) params = params.set('apenasFavorito', apenasFavorito.toString());
    return this.http.get<RelatorioPersonalizadoDto[]>(this.baseUrl, { params });
  }

  obterTemplates(): Observable<RelatorioTemplateDto[]> {
    return this.http.get<RelatorioTemplateDto[]>(`${this.baseUrl}/templates`);
  }

  obterPorId(id: string): Observable<RelatorioPersonalizadoDto> {
    return this.http.get<RelatorioPersonalizadoDto>(`${this.baseUrl}/${id}`);
  }

  criar(command: CriarRelatorioCommand): Observable<RelatorioPersonalizadoDto> {
    return this.http.post<RelatorioPersonalizadoDto>(this.baseUrl, command);
  }

  atualizar(id: string, request: AtualizarRelatorioRequest): Observable<RelatorioPersonalizadoDto> {
    return this.http.put<RelatorioPersonalizadoDto>(`${this.baseUrl}/${id}`, request);
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  marcarFavorito(id: string, favorito: boolean): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/favorito`, { favorito });
  }

  executarAdHoc(request: ExecutarRelatorioRequest): Observable<RelatorioResultadoDto> {
    return this.http.post<RelatorioResultadoDto>(`${this.baseUrl}/executar`, request);
  }

  executarSalvo(id: string): Observable<RelatorioResultadoDto> {
    return this.http.get<RelatorioResultadoDto>(`${this.baseUrl}/${id}/executar`);
  }

  exportarSalvo(id: string, formato: FormatoExportacao): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/exportar`, {
      params: new HttpParams().set('formato', formato.toString()),
      responseType: 'blob'
    });
  }

  exportarAdHoc(request: ExportarRelatorioRequest): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/exportar`, request, { responseType: 'blob' });
  }
}
