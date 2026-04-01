using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.NotasFiscais.DTOs;

public record NotaFiscalDto(
    Guid Id,
    string? NumeroNfse,
    Guid PacienteId,
    string PacienteNome,
    decimal ValorServico,
    string DescricaoServico,
    DateOnly Competencia,
    DateTimeOffset? DataEmissao,
    StatusNfse Status,
    string? ErroMensagem,
    string? PdfUrl,
    DateTimeOffset CriadoEm);
