using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Contratos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Contratos.Commands.AtualizarContrato;

public class AtualizarContratoCommandHandler : IRequestHandler<AtualizarContratoCommand, ContratoDto>
{
    private readonly IAppDbContext _context;

    public AtualizarContratoCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ContratoDto> Handle(AtualizarContratoCommand request, CancellationToken cancellationToken)
    {
        var contrato = await _context.Contratos
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Contrato não encontrado.");

        if (contrato.Status == StatusContrato.Encerrado)
            throw new InvalidOperationException("Não é possível atualizar um contrato encerrado.");

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

        // Verificar duplicidade (excluindo o próprio)
        var duplicado = await _context.Contratos
            .AnyAsync(c =>
                c.Id != request.Id &&
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

        contrato.PacienteId = request.PacienteId;
        contrato.PsicologoId = request.PsicologoId;
        contrato.ValorSessao = request.ValorSessao;
        contrato.FormaPagamento = request.FormaPagamento;
        contrato.Frequencia = request.Frequencia;
        contrato.DiaSemanasessao = request.DiaSemanaSessao;
        contrato.HorarioSessao = request.HorarioSessao;
        contrato.DuracaoMinutos = request.DuracaoMinutos;
        contrato.CobraFaltaInjustificada = request.CobraFaltaInjustificada;
        contrato.CobraFaltaJustificada = request.CobraFaltaJustificada;
        contrato.DataInicio = request.DataInicio;
        contrato.DataFim = request.DataFim;
        contrato.PlanoContaId = request.PlanoContaId;
        contrato.Observacoes = request.Observacoes;

        await _context.SaveChangesAsync(cancellationToken);

        return new ContratoDto(
            contrato.Id, contrato.PacienteId, paciente.Nome,
            contrato.PsicologoId, psicologo.Nome,
            contrato.ValorSessao, contrato.FormaPagamento.ToString(),
            contrato.Frequencia.ToString(), contrato.DiaSemanasessao.ToString(),
            contrato.HorarioSessao, contrato.DuracaoMinutos,
            contrato.CobraFaltaInjustificada, contrato.CobraFaltaJustificada,
            contrato.DataInicio, contrato.DataFim, contrato.Status.ToString(),
            contrato.MotivoEncerramento, contrato.PlanoContaId,
            contrato.Observacoes, contrato.CriadoEm);
    }
}
