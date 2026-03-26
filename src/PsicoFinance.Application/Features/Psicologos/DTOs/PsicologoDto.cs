using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Psicologos.DTOs;

public record PsicologoDto(
    Guid Id,
    string Nome,
    string Crp,
    string? Email,
    string? Telefone,
    string? Cpf,
    TipoPsicologo Tipo,
    TipoRepasse TipoRepasse,
    decimal ValorRepasse,
    string? Banco,
    string? Agencia,
    string? Conta,
    string? PixChave,
    bool Ativo,
    DateTimeOffset CriadoEm);

public record PsicologoResumoDto(
    Guid Id,
    string Nome,
    string Crp,
    TipoPsicologo Tipo,
    TipoRepasse TipoRepasse,
    decimal ValorRepasse,
    bool Ativo);
