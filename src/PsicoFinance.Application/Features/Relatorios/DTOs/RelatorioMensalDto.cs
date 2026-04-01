namespace PsicoFinance.Application.Features.Relatorios.DTOs;

public record RelatorioMensalDto(
    Guid Id,
    string PsicologoNome,
    string Competencia,
    int TotalSessoes,
    decimal ReceitaTotal,
    decimal ValorRepasse,
    string ArquivoUrl,
    DateTimeOffset CriadoEm);
