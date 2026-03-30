using MediatR;
using PsicoFinance.Application.Features.Repasses.DTOs;

namespace PsicoFinance.Application.Features.Repasses.Commands.PagarRepasse;

public record PagarRepasseCommand(Guid Id, DateOnly DataPagamento, string? Observacao) : IRequest<RepasseDto>;
