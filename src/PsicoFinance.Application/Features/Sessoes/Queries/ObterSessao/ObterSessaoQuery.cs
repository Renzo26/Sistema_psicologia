using MediatR;
using PsicoFinance.Application.Features.Sessoes.DTOs;

namespace PsicoFinance.Application.Features.Sessoes.Queries.ObterSessao;

public record ObterSessaoQuery(Guid Id) : IRequest<SessaoDto>;
