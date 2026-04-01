using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Dashboard.Queries.ObterKpisDashboard;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Dashboard;

public class ObterKpisDashboardQueryHandlerTests
{
    private static readonly Guid ClinicaId = Guid.NewGuid();
    private static readonly Guid PsicologoId = Guid.NewGuid();
    private static readonly Guid PacienteId = Guid.NewGuid();
    private const string Competencia = "2025-03";

    private static (IAppDbContext ctx, ITenantProvider tp) Setup(
        List<LancamentoFinanceiro>? lancamentos = null,
        List<Sessao>? sessoes = null,
        List<Paciente>? pacientes = null)
    {
        lancamentos ??= [];
        sessoes ??= [];
        pacientes ??= [];

        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);

        ctx.LancamentosFinanceiros.Returns(MockDbSetHelper.CreateMockDbSet(lancamentos.AsQueryable()));
        ctx.Sessoes.Returns(MockDbSetHelper.CreateMockDbSet(sessoes.AsQueryable()));
        ctx.Pacientes.Returns(MockDbSetHelper.CreateMockDbSet(pacientes.AsQueryable()));

        return (ctx, tp);
    }

    private static LancamentoFinanceiro CriarLancamento(
        TipoLancamento tipo, StatusLancamento status, decimal valor,
        DateOnly? vencimento = null, Guid? sessaoId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            ClinicaId = ClinicaId,
            Tipo = tipo,
            Status = status,
            Valor = valor,
            Competencia = Competencia,
            DataVencimento = vencimento ?? new DateOnly(2025, 3, 15),
            Descricao = "Teste",
            PlanoContaId = Guid.NewGuid(),
            SessaoId = sessaoId,
        };

    private static Sessao CriarSessao(StatusSessao status, Guid? psicologoId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            ClinicaId = ClinicaId,
            PsicologoId = psicologoId ?? PsicologoId,
            PacienteId = PacienteId,
            ContratoId = Guid.NewGuid(),
            Data = new DateOnly(2025, 3, 10),
            Status = status,
            Psicologo = new Psicologo { Id = psicologoId ?? PsicologoId, Nome = "Dr. Ana", ClinicaId = ClinicaId },
            Paciente = new Paciente { Id = PacienteId, Nome = "João Silva", ClinicaId = ClinicaId },
        };

    [Fact]
    public async Task Handle_SemTenant_LancaUnauthorizedAccessException()
    {
        var (ctx, tp) = Setup();
        tp.ClinicaId.Returns((Guid?)null);
        var handler = new ObterKpisDashboardQueryHandler(ctx, tp);

        var act = () => handler.Handle(new ObterKpisDashboardQuery(Competencia), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_SemDados_RetornaKpisZerados()
    {
        var (ctx, tp) = Setup();
        var handler = new ObterKpisDashboardQueryHandler(ctx, tp);

        var result = await handler.Handle(new ObterKpisDashboardQuery(Competencia), CancellationToken.None);

        result.Should().NotBeNull();
        result.ReceitaRealizada.Should().Be(0);
        result.DespesaRealizada.Should().Be(0);
        result.TotalSessoesAgendadas.Should().Be(0);
        result.TaxaAbsenteismo.Should().Be(0);
        result.TaxaInadimplencia.Should().Be(0);
        result.RankingPsicologos.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ComSessoesRealizadas_CalculaTaxaOcupacaoCorretamente()
    {
        var sessoes = new List<Sessao>
        {
            CriarSessao(StatusSessao.Realizada),
            CriarSessao(StatusSessao.Realizada),
            CriarSessao(StatusSessao.Falta),
            CriarSessao(StatusSessao.Cancelada),
        };
        var (ctx, tp) = Setup(sessoes: sessoes);
        var handler = new ObterKpisDashboardQueryHandler(ctx, tp);

        var result = await handler.Handle(new ObterKpisDashboardQuery(Competencia), CancellationToken.None);

        // Agendadas = 3 (excluindo cancelada), realizadas = 2, faltas = 1
        result.TotalSessoesAgendadas.Should().Be(3);
        result.TotalSessoesRealizadas.Should().Be(2);
        result.TotalSessoesFalta.Should().Be(1);
        result.TotalSessoesCanceladas.Should().Be(1);
        result.TaxaAbsenteismo.Should().Be(Math.Round(1m / 3m * 100, 2));
        result.TaxaOcupacao.Should().Be(Math.Round(2m / 3m * 100, 2));
    }

    [Fact]
    public async Task Handle_ComLancamentosConfirmados_CalculaReceitasETicketMedio()
    {
        var sessao1 = CriarSessao(StatusSessao.Realizada);
        var sessao2 = CriarSessao(StatusSessao.Realizada);

        var lancamentos = new List<LancamentoFinanceiro>
        {
            CriarLancamento(TipoLancamento.Receita, StatusLancamento.Confirmado, 200m, sessaoId: sessao1.Id),
            CriarLancamento(TipoLancamento.Receita, StatusLancamento.Confirmado, 150m, sessaoId: sessao2.Id),
            CriarLancamento(TipoLancamento.Despesa, StatusLancamento.Confirmado, 100m),
        };
        var (ctx, tp) = Setup(lancamentos, [sessao1, sessao2]);
        var handler = new ObterKpisDashboardQueryHandler(ctx, tp);

        var result = await handler.Handle(new ObterKpisDashboardQuery(Competencia), CancellationToken.None);

        result.ReceitaRealizada.Should().Be(350m);
        result.ReceitaPrevista.Should().Be(350m);
        result.DespesaRealizada.Should().Be(100m);
        result.SaldoRealizado.Should().Be(250m);
        result.TicketMedio.Should().Be(175m); // 350 / 2 sessões realizadas
    }

    [Fact]
    public async Task Handle_ComLancamentosVencidos_CalculaTaxaInadimplencia()
    {
        // Lançamento vencido (data passada)
        var vencido = CriarLancamento(
            TipoLancamento.Receita, StatusLancamento.Previsto, 200m,
            vencimento: new DateOnly(2020, 1, 1));
        var confirmado = CriarLancamento(
            TipoLancamento.Receita, StatusLancamento.Confirmado, 200m);

        var (ctx, tp) = Setup([vencido, confirmado]);
        var handler = new ObterKpisDashboardQueryHandler(ctx, tp);

        var result = await handler.Handle(new ObterKpisDashboardQuery(Competencia), CancellationToken.None);

        // 200 vencido / 400 total previsto = 50%
        result.TaxaInadimplencia.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_CompetenciaNula_UsaMesAtual()
    {
        var (ctx, tp) = Setup();
        var handler = new ObterKpisDashboardQueryHandler(ctx, tp);

        var result = await handler.Handle(new ObterKpisDashboardQuery(null), CancellationToken.None);

        var esperado = $"{DateTime.UtcNow.Year:D4}-{DateTime.UtcNow.Month:D2}";
        result.Competencia.Should().Be(esperado);
    }
}
