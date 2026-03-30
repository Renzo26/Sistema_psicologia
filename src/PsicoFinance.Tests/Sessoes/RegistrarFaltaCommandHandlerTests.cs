using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Sessoes.Commands.RegistrarFalta;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Sessoes;

public class RegistrarFaltaCommandHandlerTests
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

    private static Sessao SessaoAgendada() => new()
    {
        Id = SessaoId, ClinicaId = ClinicaId,
        Data = DateOnly.FromDateTime(DateTime.Today),
        Status = StatusSessao.Agendada,
    };

    [Fact]
    public async Task Handle_FaltaInjustificada_MarcaStatusFalta()
    {
        var sessao = SessaoAgendada();
        var (ctx, tp) = SetupContext(sessao);
        var handler = new RegistrarFaltaCommandHandler(ctx, tp);

        await handler.Handle(new RegistrarFaltaCommand(SessaoId, false, "Não compareceu"), CancellationToken.None);

        sessao.Status.Should().Be(StatusSessao.Falta);
        sessao.MotivoFalta.Should().Be("Não compareceu");
    }

    [Fact]
    public async Task Handle_FaltaJustificada_MarcaStatusFaltaJustificada()
    {
        var sessao = SessaoAgendada();
        var (ctx, tp) = SetupContext(sessao);
        var handler = new RegistrarFaltaCommandHandler(ctx, tp);

        await handler.Handle(new RegistrarFaltaCommand(SessaoId, true, "Atestado médico"), CancellationToken.None);

        sessao.Status.Should().Be(StatusSessao.FaltaJustificada);
        sessao.MotivoFalta.Should().Be("Atestado médico");
    }

    [Fact]
    public async Task Handle_SessaoCancelada_LancaInvalidOperation()
    {
        var sessao = SessaoAgendada();
        sessao.Status = StatusSessao.Cancelada;
        var (ctx, tp) = SetupContext(sessao);
        var handler = new RegistrarFaltaCommandHandler(ctx, tp);

        var act = () => handler.Handle(new RegistrarFaltaCommand(SessaoId, false, null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_SessaoInexistente_LancaKeyNotFound()
    {
        var sessoes = MockDbSetHelper.CreateMockDbSet(new List<Sessao>().AsQueryable());
        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);
        tp.UserRole.Returns((string?)null);
        ctx.Sessoes.Returns(sessoes);

        var handler = new RegistrarFaltaCommandHandler(ctx, tp);

        var act = () => handler.Handle(new RegistrarFaltaCommand(SessaoId, false, null), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
