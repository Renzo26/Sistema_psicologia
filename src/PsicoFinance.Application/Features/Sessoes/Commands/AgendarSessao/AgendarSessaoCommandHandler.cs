using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Sessoes.DTOs;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Sessoes.Commands.AgendarSessao;

public class AgendarSessaoCommandHandler : IRequestHandler<AgendarSessaoCommand, SessaoDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public AgendarSessaoCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<SessaoDto> Handle(AgendarSessaoCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var contrato = await _context.Contratos
            .AsNoTracking()
            .Include(c => c.Paciente)
            .Include(c => c.Psicologo)
            .FirstOrDefaultAsync(c => c.Id == request.ContratoId, cancellationToken)
            ?? throw new KeyNotFoundException("Contrato não encontrado.");

        if (contrato.Status != StatusContrato.Ativo)
            throw new InvalidOperationException("Apenas contratos ativos podem ter sessões agendadas.");

        var sessao = new Sessao
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            ContratoId = request.ContratoId,
            PacienteId = contrato.PacienteId,
            PsicologoId = contrato.PsicologoId,
            Data = request.Data,
            HorarioInicio = request.HorarioInicio ?? contrato.HorarioSessao,
            DuracaoMinutos = request.DuracaoMinutos ?? contrato.DuracaoMinutos,
            Status = StatusSessao.Agendada,
            Observacoes = request.Observacoes,
        };

        _context.Sessoes.Add(sessao);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(sessao, contrato.Paciente.Nome, contrato.Psicologo.Nome);
    }

    internal static SessaoDto MapToDto(Sessao s, string pacienteNome, string psicologoNome) => new(
        s.Id, s.ContratoId, s.PacienteId, pacienteNome,
        s.PsicologoId, psicologoNome,
        s.Data, s.HorarioInicio, s.DuracaoMinutos,
        s.Status.ToString(), s.Observacoes, s.MotivoFalta, s.CriadoEm);
}
