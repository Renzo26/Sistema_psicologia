using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Repasses.Commands.GerarRepasseMensal;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Repasses;

public class GerarRepasseMensalCommandHandlerTests
{
    private static readonly Guid ClinicaId = Guid.NewGuid();
    private static readonly Guid PsicologoId = Guid.NewGuid();
    private static readonly Guid ContratoId = Guid.NewGuid();

    private static Psicologo CriarPsicologoPj(TipoRepasse tipo = TipoRepasse.Percentual, decimal valor = 50m) =>
        new()
        {
            Id = PsicologoId,
            ClinicaId = ClinicaId,
            Nome = "Dr. João",
            Crp = "06/12345",
            Tipo = TipoPsicologo.Pj,
            TipoRepasse = tipo,
            ValorRepasse = valor,
            Ativo = true
        };

    private static Sessao CriarSessaoRealizada(DateOnly data) =>
        new()
        {
            Id = Guid.NewGuid(),
            ClinicaId = ClinicaId,
            PsicologoId = PsicologoId,
            Data = data,
            Status = StatusSessao.Realizada,
            ContratoId = ContratoId,
            Contrato = new Contrato { Id = ContratoId, ValorSessao = 200m, ClinicaId = ClinicaId }
        };

    private static (IAppDbContext ctx, ITenantProvider tp) Setup(
        List<Psicologo>? psicologos = null,
        List<Sessao>? sessoes = null,
        List<Repasse>? repasses = null)
    {
        psicologos ??= [CriarPsicologoPj()];
        sessoes ??= [CriarSessaoRealizada(new DateOnly(2025, 3, 10))];
        repasses ??= [];

        var psicologosSet = MockDbSetHelper.CreateMockDbSet(psicologos.AsQueryable());
        var sessoesSet = MockDbSetHelper.CreateMockDbSet(sessoes.AsQueryable());
        var repassesSet = MockDbSetHelper.CreateMockDbSet(repasses.AsQueryable());

        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);

        ctx.Psicologos.Returns(psicologosSet);
        ctx.Sessoes.Returns(sessoesSet);
        ctx.Repasses.Returns(repassesSet);
        ctx.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        return (ctx, tp);
    }

    [Fact]
    public async Task Handle_PsicologoPjComPercentual_CalculaValorCorretamente()
    {
        // 50% de 200 = 100
        var (ctx, tp) = Setup(psicologos: [CriarPsicologoPj(TipoRepasse.Percentual, 50m)]);
        var handler = new GerarRepasseMensalCommandHandler(ctx, tp);

        var result = await handler.Handle(
            new GerarRepasseMensalCommand("2025-03"), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].ValorCalculado.Should().Be(100m);
        result[0].TotalSessoes.Should().Be(1);
        result[0].MesReferencia.Should().Be("2025-03");
    }

    [Fact]
    public async Task Handle_PsicologoPjComValorFixo_CalculaValorCorretamente()
    {
        // 80 fixo * 1 sessão = 80
        var (ctx, tp) = Setup(psicologos: [CriarPsicologoPj(TipoRepasse.ValorFixo, 80m)]);
        var handler = new GerarRepasseMensalCommandHandler(ctx, tp);

        var result = await handler.Handle(
            new GerarRepasseMensalCommand("2025-03"), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].ValorCalculado.Should().Be(80m);
    }

    [Fact]
    public async Task Handle_SemSessoes_RetornaListaVazia()
    {
        var (ctx, tp) = Setup(sessoes: []);
        var handler = new GerarRepasseMensalCommandHandler(ctx, tp);

        var result = await handler.Handle(
            new GerarRepasseMensalCommand("2025-03"), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RepasseJaExiste_NaoDuplica()
    {
        var repasseExistente = new Repasse
        {
            Id = Guid.NewGuid(),
            ClinicaId = ClinicaId,
            PsicologoId = PsicologoId,
            MesReferencia = "2025-03",
            ValorCalculado = 100m,
            TotalSessoes = 1,
            Status = StatusRepasse.Pendente
        };
        var (ctx, tp) = Setup(repasses: [repasseExistente]);
        var handler = new GerarRepasseMensalCommandHandler(ctx, tp);

        var result = await handler.Handle(
            new GerarRepasseMensalCommand("2025-03"), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SemTenant_LancaUnauthorizedAccessException()
    {
        var (ctx, tp) = Setup();
        tp.ClinicaId.Returns((Guid?)null);
        var handler = new GerarRepasseMensalCommandHandler(ctx, tp);

        var act = () => handler.Handle(new GerarRepasseMensalCommand("2025-03"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_MesForaDoIntervalo_NaoIncluiSessoes()
    {
        // Sessão em fevereiro, mas pedindo março
        var sessaoFevereiro = CriarSessaoRealizada(new DateOnly(2025, 2, 15));
        var (ctx, tp) = Setup(sessoes: [sessaoFevereiro]);
        var handler = new GerarRepasseMensalCommandHandler(ctx, tp);

        var result = await handler.Handle(
            new GerarRepasseMensalCommand("2025-03"), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
