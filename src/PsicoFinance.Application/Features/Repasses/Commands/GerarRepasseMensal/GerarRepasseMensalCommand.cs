using MediatR;
using PsicoFinance.Application.Features.Repasses.DTOs;

namespace PsicoFinance.Application.Features.Repasses.Commands.GerarRepasseMensal;

public record GerarRepasseMensalCommand(
    string MesReferencia,
    Guid? PsicologoId = null) : IRequest<List<RepasseDto>>;
