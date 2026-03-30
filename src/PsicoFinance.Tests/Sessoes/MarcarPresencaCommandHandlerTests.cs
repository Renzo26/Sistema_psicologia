using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Sessoes.Commands.MarcarPresenca;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Sessoes;

public class MarcarPresencaCommandHandlerTests
{
    private static readonly Guid ClinicaId = Guid.NewGuid();
    private static readonly Guid SessaoId = Guid.NewGuid();

    private static (IAppDbContext ctx, ITenantProvider tp) SetupContext(Sessao sessao)
    {
        var sessoes = MockDbSetHelper.CreateMockDbSet(new List<Sessao> { sessao }.AsQueryable());
        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);
        tp.UserRole.Returns((string?)null);
        ctx.Sessoes.Returns(sessoes);
        ctx.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        return (ctx, tp);
    }

    private static Sessao SessaoAgendada(DateOnly? data = null) => new()
    {
        Id = SessaoId, ClinicaId = ClinicaId,
        Data = data ?? DateOnly.FromDateTime(DateTime.Today),
        Status = StatusSessao.Agendada,
    };

    [Fact]
    public async Task Handle_SessaoAgendada_MarcaComoRealizada()
    {
        var sessao = SessaoAgendada();
        var (ctx, tp) = SetupContext(sessao);
        var handler = new MarcarPresencaCommandHandler(ctx, tp);

        await handler.Handle(new MarcarPresencaCommand(SessaoId), CancellationToken.None);

        sessao.Status.Should().Be(StatusSessao.Realizada);
    }

    [Fact]
    public async Task Handle_SessaoJaRealizada_LancaInvalidOperation()
    {
        var sessao = SessaoAgendada() with { Status = StatusSessao.Realizada };
        var (ctx, tp) = SetupContext(sessao);
        var handler = new MarcarPresencaCommandHandler(ctx, tp);

        var act = () => handler.Handle(new MarcarPresencaCommand(SessaoId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*já foi marcada*");
    }

    [Fact]
    public async Task Handle_SessaoCancelada_LancaInvalidOperation()
    {
        var sessao = SessaoAgendada() with { Status = StatusSessao.Cancelada };
        var (ctx, tp) = SetupContext(sessao);
        var handler = new MarcarPresencaCommandHandler(ctx, tp);

        var act = () => handler.Handle(new MarcarPresencaCommand(SessaoId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*cancelada*");
    }

    [Fact]
    public async Task Handle_SessaoMaisde30Dias_NaoAdmin_LancaInvalidOperation()
    {
        var sessao = SessaoAgendada(DateOnly.FromDateTime(DateTime.Today.AddDays(-31)));
        var (ctx, tp) = SetupContext(sessao);
        var handler = new MarcarPresencaCommandHandler(ctx, tp);

        var act = () => handler.Handle(new MarcarPresencaCommand(SessaoId), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*30 dias*");
    }

    [Fact]
    public async Task Handle_SessaoMaisde30Dias_Admin_Permite()
    {
        var sessao = SessaoAgendada(DateOnly.FromDateTime(DateTime.Today.AddDays(-31)));
        var sessoes = MockDbSetHelper.CreateMockDbSet(new List<Sessao> { sessao }.AsQueryable());
        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);
        tp.UserRole.Returns("Admin");
        ctx.Sessoes.Returns(sessoes);
        ctx.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new MarcarPresencaCommandHandler(ctx, tp);

        await handler.Handle(new MarcarPresencaCommand(SessaoId), CancellationToken.None);

        sessao.Status.Should().Be(StatusSessao.Realizada);
    }
}
