import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ReciboDto {
  id: string;
  numeroRecibo: string;
  sessaoId: string;
  pacienteNome: string;
  psicologoNome: string;
  valor: number;
  dataEmissao: string;
  status: 'Gerado' | 'Cancelado';
  arquivoUrl: string | null;
  criadoEm: string;
}

export interface NotaFiscalDto {
  id: string;
  numeroNfse: string | null;
  pacienteId: string;
  pacienteNome: string;
  valorServico: number;
  descricaoServico: string;
  competencia: string;
  dataEmissao: string | null;
  status: 'Pendente' | 'Emitida' | 'Cancelada' | 'Erro';
  erroMensagem: string | null;
  pdfUrl: string | null;
  criadoEm: string;
}

export interface RelatorioMensalDto {
  id: string;
  psicologoNome: string;
  competencia: string;
  totalSessoes: number;
  receitaTotal: number;
  valorRepasse: number;
  arquivoUrl: string;
  geradoEm: string;
}

export interface PsicologoResumo {
  id: string;
  nome: string;
}

export interface RecibosFilter {
  pacienteId?: string;
  dataInicio?: string;
  dataFim?: string;
  status?: string;
}

export interface NotasFiscaisFilter {
  pacienteId?: string;
  competenciaInicio?: string;
  competenciaFim?: string;
  status?: string;
}

export interface EmitirReciboCommand {
  sessaoId: string;
}

export interface EmitirNotaFiscalCommand {
  pacienteId: string;
  lancamentoId?: string | null;
  valorServico: number;
  descricaoServico: string;
  competencia: string;
}

export interface GerarRelatorioMensalCommand {
  psicologoId: string;
  competencia: string;
}

@Injectable({ providedIn: 'root' })
export class DocumentosService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // --- Recibos ---

  getRecibos(filter?: RecibosFilter): Observable<ReciboDto[]> {
    let params = new HttpParams();
    if (filter?.pacienteId) params = params.set('pacienteId', filter.pacienteId);
    if (filter?.dataInicio) params = params.set('dataInicio', filter.dataInicio);
    if (filter?.dataFim) params = params.set('dataFim', filter.dataFim);
    if (filter?.status) params = params.set('status', filter.status);
    return this.http.get<ReciboDto[]>(`${this.baseUrl}/recibos`, { params });
  }

  emitirRecibo(command: EmitirReciboCommand): Observable<ReciboDto> {
    return this.http.post<ReciboDto>(`${this.baseUrl}/recibos`, command);
  }

  downloadReciboPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/recibos/${id}/pdf`, { responseType: 'blob' });
  }

  cancelarRecibo(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/recibos/${id}/cancelar`, {});
  }

  // --- Notas Fiscais ---

  getNotasFiscais(filter?: NotasFiscaisFilter): Observable<NotaFiscalDto[]> {
    let params = new HttpParams();
    if (filter?.pacienteId) params = params.set('pacienteId', filter.pacienteId);
    if (filter?.competenciaInicio) params = params.set('competenciaInicio', filter.competenciaInicio);
    if (filter?.competenciaFim) params = params.set('competenciaFim', filter.competenciaFim);
    if (filter?.status) params = params.set('status', filter.status);
    return this.http.get<NotaFiscalDto[]>(`${this.baseUrl}/notas-fiscais`, { params });
  }

  emitirNotaFiscal(command: EmitirNotaFiscalCommand): Observable<NotaFiscalDto> {
    return this.http.post<NotaFiscalDto>(`${this.baseUrl}/notas-fiscais`, command);
  }

  cancelarNotaFiscal(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/notas-fiscais/${id}/cancelar`, {});
  }

  // --- Relatórios Mensais ---

  gerarRelatorioMensal(command: GerarRelatorioMensalCommand): Observable<RelatorioMensalDto> {
    return this.http.post<RelatorioMensalDto>(`${this.baseUrl}/relatorios/mensal`, command);
  }

  downloadRelatorio(filePath: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/relatorios/mensal/${filePath}`, { responseType: 'blob' });
  }

  // --- Psicólogos (para select) ---

  getPsicologos(): Observable<PsicologoResumo[]> {
    return this.http.get<PsicologoResumo[]>(`${this.baseUrl}/psicologos`);
  }
}
