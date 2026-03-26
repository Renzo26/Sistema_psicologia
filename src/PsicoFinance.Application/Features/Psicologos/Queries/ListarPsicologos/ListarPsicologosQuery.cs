using MediatR;
using PsicoFinance.Application.Features.Psicologos.DTOs;

namespace PsicoFinance.Application.Features.Psicologos.Queries.ListarPsicologos;

public record ListarPsicologosQuery(
    string? Busca = null,
    bool? ApenasAtivos = true) : IRequest<List<PsicologoResumoDto>>;
