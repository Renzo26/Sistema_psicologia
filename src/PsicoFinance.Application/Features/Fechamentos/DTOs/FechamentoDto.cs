using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Fechamentos.DTOs;

public record FechamentoDto(
    Guid Id,
    string MesReferencia,
    StatusFechamento Status,
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo,
    int TotalSessoes,
    int TotalSessoesRealizadas,
    int TotalSessoesFalta,
    DateTimeOffset? FechadoEm,
    string? Observacao,
    List<FechamentoRepasseConsolidadoDto> RepassesPorPsicologo);

public record FechamentoRepasseConsolidadoDto(
    Guid PsicologoId,
    string PsicologoNome,
    int TotalSessoes,
    decimal ReceitaGerada,
    decimal ValorRepasse);
