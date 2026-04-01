using MediatR;
using PsicoFinance.Application.Features.Relatorios.DTOs;

namespace PsicoFinance.Application.Features.Relatorios.Commands.GerarRelatorioMensal;

public record GerarRelatorioMensalCommand(
    Guid PsicologoId,
    string Competencia) : IRequest<RelatorioMensalDto>;
