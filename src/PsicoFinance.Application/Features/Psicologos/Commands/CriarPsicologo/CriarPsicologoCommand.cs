using MediatR;
using PsicoFinance.Application.Features.Psicologos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Psicologos.Commands.CriarPsicologo;

public record CriarPsicologoCommand(
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
    string? PixChave) : IRequest<PsicologoDto>;
