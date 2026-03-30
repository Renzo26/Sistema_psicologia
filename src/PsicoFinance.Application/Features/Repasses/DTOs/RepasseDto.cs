using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Repasses.DTOs;

public record RepasseDto(
    Guid Id,
    Guid PsicologoId,
    string PsicologoNome,
    string MesReferencia,
    decimal ValorCalculado,
    int TotalSessoes,
    StatusRepasse Status,
    DateOnly? DataPagamento,
    string? Observacao,
    DateTimeOffset CriadoEm);
