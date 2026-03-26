using MediatR;
using PsicoFinance.Application.Features.Psicologos.DTOs;

namespace PsicoFinance.Application.Features.Psicologos.Queries.ObterPsicologo;

public record ObterPsicologoQuery(Guid Id) : IRequest<PsicologoDto>;
