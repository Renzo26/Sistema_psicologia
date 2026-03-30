using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Sessoes.Commands.AgendarSessao;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Sessoes;

public class AgendarSessaoCommandHandlerTests
{
    private static readonly Guid ClinicaId = Guid.NewGuid();
    private static readonly Guid ContratoId = Guid.NewGuid();
    private static readonly Guid PacienteId = Guid.NewGuid();
    private static readonly Guid PsicologoId = Guid.NewGuid();

    private static AgendarSessaoCommand Cmd() => new(
        ContratoId: ContratoId,
        Data: DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
        HorarioInicio: null,
        DuracaoMinutos: null,
        Observacoes: null);

    private static (IAppDbContext ctx, ITenantProvider tp) SetupContext(
        Contrato? contrato = null)
    {
        contrato ??= new Contrato
        {
            Id = ContratoId, ClinicaId = ClinicaId,
            PacienteId = PacienteId, PsicologoId = PsicologoId,
            Status = StatusContrato.Ativo,
            HorarioSessao = new TimeOnly(14, 0),
            DuracaoMinutos = 50,
            Paciente = new Paciente { Id = PacienteId, Nome = "Maria Silva", ClinicaId = ClinicaId },
            Psicologo = new Psicologo { Id = PsicologoId, Nome = "Dr. João", Crp = "06/1234", ClinicaId = ClinicaId },
        };

        var contratos = MockDbSetHelper.CreateMockDbSet(new List<Contrato> { contrato }.AsQueryable());
        var sessoes = MockDbSetHelper.CreateMockDbSet(new List<Sessao>().AsQueryable());

        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);

        ctx.Contratos.Returns(contratos);
        ctx.Sessoes.Returns(sessoes);
        ctx.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        return (ctx, tp);
    }

    [Fact]
    public async Task Handle_DadosValidos_AgendaSessao()
    {
        var (ctx, tp) = SetupContext();
        var handler = new AgendarSessaoCommandHandler(ctx, tp);

        var result = await handler.Handle(Cmd(), CancellationToken.None);

        result.PacienteNome.Should().Be("Maria Silva");
        result.PsicologoNome.Should().Be("Dr. João");
        result.Status.Should().Be("Agendada");
        result.HorarioInicio.Should().Be(new TimeOnly(14, 0));
        result.DuracaoMinutos.Should().Be(50);
    }

    [Fact]
    public async Task Handle_TenantNulo_LancaUnauthorized()
    {
        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns((Guid?)null);

        var handler = new AgendarSessaoCommandHandler(ctx, tp);
        var act = () => handler.Handle(Cmd(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ContratoInexistente_LancaKeyNotFound()
    {
        var contratos = MockDbSetHelper.CreateMockDbSet(new List<Contrato>().AsQueryable());
        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);
        ctx.Contratos.Returns(contratos);

        var handler = new AgendarSessaoCommandHandler(ctx, tp);
        var act = () => handler.Handle(Cmd(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Contrato*");
    }

    [Fact]
    public async Task Handle_ContratoEncerrado_LancaInvalidOperation()
    {
        var contrato = new Contrato
        {
            Id = ContratoId, ClinicaId = ClinicaId,
            PacienteId = PacienteId, PsicologoId = PsicologoId,
            Status = StatusContrato.Encerrado,
            HorarioSessao = new TimeOnly(14, 0), DuracaoMinutos = 50,
            Paciente = new Paciente { Id = PacienteId, Nome = "Maria", ClinicaId = ClinicaId },
            Psicologo = new Psicologo { Id = PsicologoId, Nome = "Dr. João", Crp = "06/1234", ClinicaId = ClinicaId },
        };
        var (ctx, tp) = SetupContext(contrato);

        var handler = new AgendarSessaoCommandHandler(ctx, tp);
        var act = () => handler.Handle(Cmd(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*contratos ativos*");
    }

    [Fact]
    public async Task Handle_HorarioCustomizado_UsaHorarioInformado()
    {
        var (ctx, tp) = SetupContext();
        var handler = new AgendarSessaoCommandHandler(ctx, tp);
        var cmd = Cmd() with { HorarioInicio = new TimeOnly(16, 30) };

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.HorarioInicio.Should().Be(new TimeOnly(16, 30));
    }
}
