using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Contratos.Commands.EncerrarContrato;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Contratos;

public class EncerrarContratoCommandHandlerTests
{
    private static readonly Guid ContratoId = Guid.NewGuid();
    private static readonly Guid ClinicaId = Guid.NewGuid();

    private static IAppDbContext SetupContext(List<Contrato> contratos)
    {
        var contratosSet = MockDbSetHelper.CreateMockDbSet(contratos.AsQueryable());
        var ctx = Substitute.For<IAppDbContext>();
        ctx.Contratos.Returns(contratosSet);
        ctx.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        return ctx;
    }

    private static Contrato CriarContrato(StatusContrato status) => new()
    {
        Id = ContratoId, ClinicaId = ClinicaId,
        PacienteId = Guid.NewGuid(), PsicologoId = Guid.NewGuid(),
        Status = status, ValorSessao = 150m,
        DiaSemanasessao = DiaSemana.Segunda,
        HorarioSessao = new TimeOnly(14, 0),
        DataInicio = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3))
    };

    [Fact]
    public async Task Handle_ContratoAtivo_EncerraComSucesso()
    {
        var contrato = CriarContrato(StatusContrato.Ativo);
        var ctx = SetupContext(new List<Contrato> { contrato });

        var handler = new EncerrarContratoCommandHandler(ctx);
        await handler.Handle(new EncerrarContratoCommand(ContratoId, "Fim do tratamento"), CancellationToken.None);

        contrato.Status.Should().Be(StatusContrato.Encerrado);
        contrato.MotivoEncerramento.Should().Be("Fim do tratamento");
        contrato.DataFim.Should().Be(DateOnly.FromDateTime(DateTime.Today));
    }

    [Fact]
    public async Task Handle_ContratoJaEncerrado_LancaInvalidOperation()
    {
        var contrato = CriarContrato(StatusContrato.Encerrado);
        var ctx = SetupContext(new List<Contrato> { contrato });

        var handler = new EncerrarContratoCommandHandler(ctx);
        var act = () => handler.Handle(new EncerrarContratoCommand(ContratoId, null), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*já está encerrado*");
    }

    [Fact]
    public async Task Handle_ContratoInexistente_LancaKeyNotFound()
    {
        var ctx = SetupContext(new List<Contrato>());

        var handler = new EncerrarContratoCommandHandler(ctx);
        var act = () => handler.Handle(new EncerrarContratoCommand(Guid.NewGuid(), null), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
