using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Contratos.DTOs;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Contratos.Commands.CriarContrato;

public class CriarContratoCommandHandler : IRequestHandler<CriarContratoCommand, ContratoDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CriarContratoCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<ContratoDto> Handle(CriarContratoCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        // Verificar se paciente existe
        var paciente = await _context.Pacientes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PacienteId, cancellationToken)
            ?? throw new KeyNotFoundException("Paciente não encontrado.");

        // Verificar se psicólogo existe
        var psicologo = await _context.Psicologos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PsicologoId, cancellationToken)
            ?? throw new KeyNotFoundException("Psicólogo não encontrado.");

        // Verificar se já existe contrato ativo para mesmo paciente + psicólogo + dia/horário
        var duplicado = await _context.Contratos
            .AnyAsync(c =>
                c.PacienteId == request.PacienteId &&
                c.PsicologoId == request.PsicologoId &&
                c.DiaSemanasessao == request.DiaSemanaSessao &&
                c.HorarioSessao == request.HorarioSessao &&
                c.Status == StatusContrato.Ativo,
                cancellationToken);

        if (duplicado)
            throw new InvalidOperationException("Já existe um contrato ativo para este paciente e psicólogo no mesmo dia/horário.");

        // Verificar plano de conta (se informado)
        if (request.PlanoContaId.HasValue)
        {
            var planoExiste = await _context.PlanosConta
                .AnyAsync(p => p.Id == request.PlanoContaId.Value, cancellationToken);
            if (!planoExiste)
                throw new KeyNotFoundException("Plano de conta não encontrado.");
        }

        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            PacienteId = request.PacienteId,
            PsicologoId = request.PsicologoId,
            ValorSessao = request.ValorSessao,
            FormaPagamento = request.FormaPagamento,
            Frequencia = request.Frequencia,
            DiaSemanasessao = request.DiaSemanaSessao,
            HorarioSessao = request.HorarioSessao,
            DuracaoMinutos = request.DuracaoMinutos,
            CobraFaltaInjustificada = request.CobraFaltaInjustificada,
            CobraFaltaJustificada = request.CobraFaltaJustificada,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            PlanoContaId = request.PlanoContaId,
            Observacoes = request.Observacoes,
            Status = StatusContrato.Ativo
        };

        _context.Contratos.Add(contrato);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(contrato, paciente.Nome, psicologo.Nome);
    }

    private static ContratoDto MapToDto(Contrato c, string pacienteNome, string psicologoNome) => new(
        c.Id, c.PacienteId, pacienteNome, c.PsicologoId, psicologoNome,
        c.ValorSessao, c.FormaPagamento.ToString(), c.Frequencia.ToString(),
        c.DiaSemanasessao.ToString(), c.HorarioSessao, c.DuracaoMinutos,
        c.CobraFaltaInjustificada, c.CobraFaltaJustificada,
        c.DataInicio, c.DataFim, c.Status.ToString(),
        c.MotivoEncerramento, c.PlanoContaId, c.Observacoes, c.CriadoEm);
}
