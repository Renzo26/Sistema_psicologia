using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.PlanosConta.Commands.CriarPlanoConta;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.PlanosConta;

public class CriarPlanoContaCommandHandlerTests
{
    private static readonly Guid ClinicaId = Guid.NewGuid();

    private static (IAppDbContext ctx, ITenantProvider tp) Setup(List<PlanoConta>? planos = null)
    {
        planos ??= [];
        var planosSet = MockDbSetHelper.CreateMockDbSet(planos.AsQueryable());

        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);

        ctx.PlanosConta.Returns(planosSet);
        ctx.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        return (ctx, tp);
    }

    [Fact]
    public async Task Handle_CommandValido_CriaPlanoERetornaDto()
    {
        var (ctx, tp) = Setup();
        var handler = new CriarPlanoContaCommandHandler(ctx, tp);

        var result = await handler.Handle(
            new CriarPlanoContaCommand("Sessões", TipoPlanoConta.Receita, "Receitas de sessões"),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Nome.Should().Be("Sessões");
        result.Tipo.Should().Be(TipoPlanoConta.Receita);
        result.Descricao.Should().Be("Receitas de sessões");
        result.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PlanoAdicionadoAoContext()
    {
        var (ctx, tp) = Setup();
        var handler = new CriarPlanoContaCommandHandler(ctx, tp);

        await handler.Handle(
            new CriarPlanoContaCommand("Aluguel", TipoPlanoConta.Despesa, null),
            CancellationToken.None);

        ctx.PlanosConta.Received(1).Add(Arg.Is<PlanoConta>(p =>
            p.Nome == "Aluguel" &&
            p.Tipo == TipoPlanoConta.Despesa &&
            p.ClinicaId == ClinicaId));
        await ctx.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NomeDuplicadoMesmoTipo_LancaInvalidOperationException()
    {
        var planoExistente = new PlanoConta
        {
            Id = Guid.NewGuid(),
            ClinicaId = ClinicaId,
            Nome = "Sessões",
            Tipo = TipoPlanoConta.Receita,
            Ativo = true
        };
        var (ctx, tp) = Setup([planoExistente]);
        var handler = new CriarPlanoContaCommandHandler(ctx, tp);

        var act = () => handler.Handle(
            new CriarPlanoContaCommand("Sessões", TipoPlanoConta.Receita, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já existe*");
    }

    [Fact]
    public async Task Handle_SemTenant_LancaUnauthorizedAccessException()
    {
        var (ctx, tp) = Setup();
        tp.ClinicaId.Returns((Guid?)null);
        var handler = new CriarPlanoContaCommandHandler(ctx, tp);

        var act = () => handler.Handle(
            new CriarPlanoContaCommand("Sessões", TipoPlanoConta.Receita, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_MesmoNomeTipoDiferente_CriaComSucesso()
    {
        var planoExistente = new PlanoConta
        {
            Id = Guid.NewGuid(),
            ClinicaId = ClinicaId,
            Nome = "Sessões",
            Tipo = TipoPlanoConta.Despesa,
            Ativo = true
        };
        var (ctx, tp) = Setup([planoExistente]);
        var handler = new CriarPlanoContaCommandHandler(ctx, tp);

        var result = await handler.Handle(
            new CriarPlanoContaCommand("Sessões", TipoPlanoConta.Receita, null),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Nome.Should().Be("Sessões");
    }
}
