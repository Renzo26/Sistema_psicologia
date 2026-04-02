using PsicoFinance.Application.Features.RelatoriosBI.DTOs;

namespace PsicoFinance.Application.Common.Interfaces;

public interface IRelatorioExportService
{
    Task<byte[]> ExportarPdfAsync(RelatorioResultadoDto resultado, CancellationToken ct);
    Task<byte[]> ExportarXlsxAsync(RelatorioResultadoDto resultado, CancellationToken ct);
    byte[] ExportarCsv(RelatorioResultadoDto resultado);
}
