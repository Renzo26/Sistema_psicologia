using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Lancamentos.DTOs;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Lancamentos.Commands.CriarLancamento;

public class CriarLancamentoCommandHandler : IRequestHandler<CriarLancamentoCommand, LancamentoDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CriarLancamentoCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<LancamentoDto> Handle(CriarLancamentoCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var planoConta = await _context.PlanosConta
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PlanoContaId, cancellationToken)
            ?? throw new KeyNotFoundException("Plano de conta não encontrado.");

        if (!planoConta.Ativo)
            throw new InvalidOperationException("Plano de conta inativo.");

        if (request.SessaoId.HasValue)
        {
            var sessaoExiste = await _context.Sessoes
                .AnyAsync(s => s.Id == request.SessaoId.Value, cancellationToken);
            if (!sessaoExiste)
                throw new KeyNotFoundException("Sessão não encontrada.");
        }

        var lancamento = new LancamentoFinanceiro
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            Descricao = request.Descricao,
            Valor = request.Valor,
            Tipo = request.Tipo,
            Status = StatusLancamento.Previsto,
            DataVencimento = request.DataVencimento,
            Competencia = request.Competencia,
            PlanoContaId = request.PlanoContaId,
            SessaoId = request.SessaoId,
            Observacao = request.Observacao
        };

        _context.LancamentosFinanceiros.Add(lancamento);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(lancamento, planoConta.Nome);
    }

    internal static LancamentoDto MapToDto(LancamentoFinanceiro l, string planoContaNome) => new(
        l.Id, l.Descricao, l.Valor, l.Tipo, l.Status,
        l.DataVencimento, l.DataPagamento, l.Competencia,
        l.SessaoId, l.PlanoContaId, planoContaNome, l.Observacao, l.CriadoEm);
}
