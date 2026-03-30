using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.PlanosConta.DTOs;

public record PlanoContaDto(
    Guid Id,
    string Nome,
    TipoPlanoConta Tipo,
    string? Descricao,
    bool Ativo,
    DateTimeOffset CriadoEm);
