using MediatR;
using PsicoFinance.Application.Features.Clinicas.DTOs;

namespace PsicoFinance.Application.Features.Clinicas.Queries.ObterClinica;

/// <summary>
/// Obtém os dados da clínica do tenant atual (via TenantProvider).
/// </summary>
public record ObterClinicaQuery : IRequest<ClinicaDto>;
