using MediatR;
using PsicoFinance.Application.Features.NotasFiscais.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.NotasFiscais.Queries.ListarNotasFiscais;

public record ListarNotasFiscaisQuery(
    Guid? PacienteId,
    DateOnly? CompetenciaInicio,
    DateOnly? CompetenciaFim,
    StatusNfse? Status) : IRequest<List<NotaFiscalDto>>;
