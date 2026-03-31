using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Sessoes.DTOs;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Sessoes.Commands.GerarSessoesRecorrentes;

public class GerarSessoesRecorrentesCommandHandler
    : IRequestHandler<GerarSessoesRecorrentesCommand, List<SessaoResumoDto>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GerarSessoesRecorrentesCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<SessaoResumoDto>> Handle(
        GerarSessoesRecorrentesCommand request, CancellationToken cancellationToken)
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
            throw new InvalidOperationException("Apenas contratos ativos podem gerar sessões recorrentes.");

        // Carrega datas já agendadas para evitar duplicatas
        var datasExistentes = await _context.Sessoes
            .Where(s => s.ContratoId == request.ContratoId
                     && s.Status != StatusSessao.Cancelada)
            .Select(s => s.Data)
            .ToListAsync(cancellationToken);

        var dataFimEfetiva = request.DataFim ?? contrato.DataFim ?? request.DataInicio.AddMonths(3);
        var limite = request.QuantidadeSessoes ?? 52;
        var intervaloDias = contrato.Frequencia == FrequenciaContrato.Quinzenal ? 14 : 7;
        var diaDaSemana = MapDiaSemana(contrato.DiaSemanasessao);

        var datas = CalcularDatas(request.DataInicio, dataFimEfetiva, diaDaSemana, intervaloDias, limite);

        var novasSessoes = new List<Sessao>();
        foreach (var data in datas)
        {
            if (datasExistentes.Contains(data)) continue;

            novasSessoes.Add(new Sessao
            {
                Id = Guid.NewGuid(),
                ClinicaId = clinicaId,
                ContratoId = request.ContratoId,
                PacienteId = contrato.PacienteId,
                PsicologoId = contrato.PsicologoId,
                Data = data,
                HorarioInicio = contrato.HorarioSessao,
                DuracaoMinutos = contrato.DuracaoMinutos,
                Status = StatusSessao.Agendada,
            });
        }

        if (novasSessoes.Count == 0)
            return [];

        _context.Sessoes.AddRange(novasSessoes);

        // Se o contrato tiver plano de conta, gera lançamento Previsto para cada sessão
        if (contrato.PlanoContaId.HasValue)
        {
            var lancamentos = novasSessoes.Select(s => new LancamentoFinanceiro
            {
                Id = Guid.NewGuid(),
                ClinicaId = clinicaId,
                Descricao = $"Sessão - {contrato.Paciente.Nome} - {s.Data:dd/MM/yyyy}",
                Valor = contrato.ValorSessao,
                Tipo = TipoLancamento.Receita,
                Status = StatusLancamento.Previsto,
                DataVencimento = s.Data,
                Competencia = s.Data.ToString("yyyy-MM"),
                PlanoContaId = contrato.PlanoContaId.Value,
                SessaoId = s.Id,
            });
            _context.LancamentosFinanceiros.AddRange(lancamentos);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return novasSessoes
            .Select(s => new SessaoResumoDto(
                s.Id, s.ContratoId,
                contrato.Paciente.Nome, contrato.Psicologo.Nome,
                s.Data, s.HorarioInicio, s.DuracaoMinutos,
                s.Status.ToString()))
            .ToList();
    }

    private static List<DateOnly> CalcularDatas(
        DateOnly inicio, DateOnly fim, DayOfWeek diaSemana, int intervalo, int limite)
    {
        var datas = new List<DateOnly>();
        var atual = inicio;

        // Avança até o primeiro dia correto da semana
        while (atual.DayOfWeek != diaSemana)
            atual = atual.AddDays(1);

        while (atual <= fim && datas.Count < limite)
        {
            datas.Add(atual);
            atual = atual.AddDays(intervalo);
        }

        return datas;
    }

    private static DayOfWeek MapDiaSemana(DiaSemana dia) => dia switch
    {
        DiaSemana.Domingo  => DayOfWeek.Sunday,
        DiaSemana.Segunda  => DayOfWeek.Monday,
        DiaSemana.Terca    => DayOfWeek.Tuesday,
        DiaSemana.Quarta   => DayOfWeek.Wednesday,
        DiaSemana.Quinta   => DayOfWeek.Thursday,
        DiaSemana.Sexta    => DayOfWeek.Friday,
        DiaSemana.Sabado   => DayOfWeek.Saturday,
        _ => DayOfWeek.Monday
    };
}
