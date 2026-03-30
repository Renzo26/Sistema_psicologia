using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Lancamentos.Commands.CriarLancamento;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Lancamentos;

public class CriarLancamentoCommandHandlerTests
{
    private static readonly Guid ClinicaId = Guid.NewGuid();
    private static readonly Guid PlanoContaId = Guid.NewGuid();

    private static CriarLancamentoCommand Cmd(Guid? planoId = null) => new(
        Descricao: "Sessão João",
        Valor: 150m,
        Tipo: TipoLancamento.Receita,
        DataVencimento: DateOnly.FromDateTime(DateTime.Today),
        Competencia: "2025-03",
        PlanoContaId: planoId ?? PlanoContaId,
        SessaoId: null,
        Observacao: null);

    private static (IAppDbContext ctx, ITenantProvider tp) Setup(
        List<PlanoConta>? planos = null,
        List<Sessao>? sessoes = null)
    {
        planos ??= [new() { Id = PlanoContaId, ClinicaId = ClinicaId, Nome = "Sessões", Tipo = TipoPlanoConta.Receita, Ativo = true }];
        sessoes ??= [];

        var planosSet = MockDbSetHelper.CreateMockDbSet(planos.AsQueryable());
        var sessoesSet = MockDbSetHelper.CreateMockDbSet(sessoes.AsQueryable());
        var lancamentosSet = MockDbSetHelper.CreateMockDbSet(new List<LancamentoFinanceiro>().AsQueryable());

        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);

        ctx.PlanosConta.Returns(planosSet);
        ctx.Sessoes.Returns(sessoesSet);
        ctx.LancamentosFinanceiros.Returns(lancamentosSet);
        ctx.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        return (ctx, tp);
    }

    [Fact]
    public async Task Handle_CommandValido_CriaLancamentoERetornaDto()
    {
        var (ctx, tp) = Setup();
        var handler = new CriarLancamentoCommandHandler(ctx, tp);

        var result = await handler.Handle(Cmd(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Descricao.Should().Be("Sessão João");
        result.Valor.Should().Be(150m);
        result.Tipo.Should().Be(TipoLancamento.Receita);
        result.Status.Should().Be(StatusLancamento.Previsto);
        result.Competencia.Should().Be("2025-03");
    }

    [Fact]
    public async Task Handle_LancamentoAdicionadoAoContext()
    {
        var (ctx, tp) = Setup();
        var handler = new CriarLancamentoCommandHandler(ctx, tp);

        await handler.Handle(Cmd(), CancellationToken.None);

        ctx.LancamentosFinanceiros.Received(1).Add(Arg.Is<LancamentoFinanceiro>(l =>
            l.Descricao == "Sessão João" &&
            l.Valor == 150m &&
            l.ClinicaId == ClinicaId &&
            l.Status == StatusLancamento.Previsto));
        await ctx.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PlanoContaInexistente_LancaKeyNotFoundException()
    {
        var (ctx, tp) = Setup(planos: []);
        var handler = new CriarLancamentoCommandHandler(ctx, tp);

        var act = () => handler.Handle(Cmd(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*plano de conta*");
    }

    [Fact]
    public async Task Handle_PlanoContaInativo_LancaInvalidOperationException()
    {
        var planoInativo = new PlanoConta { Id = PlanoContaId, ClinicaId = ClinicaId, Nome = "X", Tipo = TipoPlanoConta.Receita, Ativo = false };
        var (ctx, tp) = Setup(planos: [planoInativo]);
        var handler = new CriarLancamentoCommandHandler(ctx, tp);

        var act = () => handler.Handle(Cmd(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*inativo*");
    }

    [Fact]
    public async Task Handle_SemTenant_LancaUnauthorizedAccessException()
    {
        var (ctx, tp) = Setup();
        tp.ClinicaId.Returns((Guid?)null);
        var handler = new CriarLancamentoCommandHandler(ctx, tp);

        var act = () => handler.Handle(Cmd(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
