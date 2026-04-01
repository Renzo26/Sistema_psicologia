using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Recibos.DTOs;

public record ReciboDto(
    Guid Id,
    string NumeroRecibo,
    Guid SessaoId,
    string PacienteNome,
    string PsicologoNome,
    decimal Valor,
    DateOnly DataEmissao,
    StatusRecibo Status,
    string? ArquivoUrl,
    DateTimeOffset CriadoEm);
