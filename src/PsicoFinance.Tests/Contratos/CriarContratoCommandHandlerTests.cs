using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Contratos.Commands.CriarContrato;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Contratos;

public class CriarContratoCommandHandlerTests
{
    private static readonly Guid ClinicaId = Guid.NewGuid();
    private static readonly Guid PacienteId = Guid.NewGuid();
    private static readonly Guid PsicologoId = Guid.NewGuid();

    private static CriarContratoCommand Cmd() => new(
        PacienteId: PacienteId, PsicologoId: PsicologoId,
        ValorSessao: 150m, FormaPagamento: FormaPagamento.Pix,
        Frequencia: FrequenciaContrato.Semanal, DiaSemanaSessao: DiaSemana.Segunda,
        HorarioSessao: new TimeOnly(14, 0), DuracaoMinutos: 50,
        CobraFaltaInjustificada: true, CobraFaltaJustificada: false,
        DataInicio: DateOnly.FromDateTime(DateTime.Today),
        DataFim: null, PlanoContaId: null, Observacoes: "Teste");

    private static (IAppDbContext ctx, ITenantProvider tp) SetupContext(
        List<Paciente>? pacientes = null,
        List<Psicologo>? psicologos = null,
        List<Contrato>? contratos = null,
        List<PlanoConta>? planos = null)
    {
        pacientes ??= new List<Paciente>
        {
            new() { Id = PacienteId, ClinicaId = ClinicaId, Nome = "Maria Silva", Ativo = true }
        };
        psicologos ??= new List<Psicologo>
        {
            new() { Id = PsicologoId, ClinicaId = ClinicaId, Nome = "Dr. João", Crp = "06/12345", Ativo = true }
        };
        contratos ??= new List<Contrato>();
        planos ??= new List<PlanoConta>();

        // Criar mock sets ANTES de configurar o substitute
        var pacientesSet = MockDbSetHelper.CreateMockDbSet(pacientes.AsQueryable());
        var psicologosSet = MockDbSetHelper.CreateMockDbSet(psicologos.AsQueryable());
        var contratosSet = MockDbSetHelper.CreateMockDbSet(contratos.AsQueryable());
        var planosSet = MockDbSetHelper.CreateMockDbSet(planos.AsQueryable());

        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);

        ctx.Pacientes.Returns(pacientesSet);
        ctx.Psicologos.Returns(psicologosSet);
        ctx.Contratos.Returns(contratosSet);
        ctx.PlanosConta.Returns(planosSet);
        ctx.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        return (ctx, tp);
    }

    [Fact]
    public async Task Handle_DadosValidos_CriaContrato()
    {
        var (ctx, tp) = SetupContext();
        var handler = new CriarContratoCommandHandler(ctx, tp);

        var result = await handler.Handle(Cmd(), CancellationToken.None);

        result.PacienteNome.Should().Be("Maria Silva");
        result.PsicologoNome.Should().Be("Dr. João");
        result.ValorSessao.Should().Be(150m);
        result.Status.Should().Be("Ativo");
    }

    [Fact]
    public async Task Handle_TenantNulo_LancaUnauthorized()
    {
        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns((Guid?)null);

        var handler = new CriarContratoCommandHandler(ctx, tp);
        var act = () => handler.Handle(Cmd(), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_PacienteInexistente_LancaKeyNotFound()
    {
        var (ctx, tp) = SetupContext(pacientes: new List<Paciente>());
        var handler = new CriarContratoCommandHandler(ctx, tp);

        var act = () => handler.Handle(Cmd(), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Paciente*");
    }

    [Fact]
    public async Task Handle_PsicologoInexistente_LancaKeyNotFound()
    {
        var (ctx, tp) = SetupContext(psicologos: new List<Psicologo>());
        var handler = new CriarContratoCommandHandler(ctx, tp);

        var act = () => handler.Handle(Cmd(), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*sicólogo*");
    }

    [Fact]
    public async Task Handle_ContratoDuplicado_LancaInvalidOperation()
    {
        var contratoExistente = new Contrato
        {
            Id = Guid.NewGuid(), ClinicaId = ClinicaId,
            PacienteId = PacienteId, PsicologoId = PsicologoId,
            DiaSemanasessao = DiaSemana.Segunda,
            HorarioSessao = new TimeOnly(14, 0),
            Status = StatusContrato.Ativo, ValorSessao = 100m
        };
        var (ctx, tp) = SetupContext(contratos: new List<Contrato> { contratoExistente });
        var handler = new CriarContratoCommandHandler(ctx, tp);

        var act = () => handler.Handle(Cmd(), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*contrato ativo*");
    }
}
