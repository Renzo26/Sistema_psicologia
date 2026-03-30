using MediatR;
using PsicoFinance.Application.Features.Sessoes.DTOs;

namespace PsicoFinance.Application.Features.Sessoes.Commands.GerarSessoesRecorrentes;

public record GerarSessoesRecorrentesCommand(
    Guid ContratoId,
    DateOnly DataInicio,
    DateOnly? DataFim,
    int? QuantidadeSessoes) : IRequest<List<SessaoResumoDto>>;
