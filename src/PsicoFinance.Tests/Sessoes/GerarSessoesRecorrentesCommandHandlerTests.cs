using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Sessoes.Commands.GerarSessoesRecorrentes;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Sessoes;

public class GerarSessoesRecorrentesCommandHandlerTests
{
    private static readonly Guid ClinicaId = Guid.NewGuid();
    private static readonly Guid ContratoId = Guid.NewGuid();
    private static readonly Guid PacienteId = Guid.NewGuid();
    private static readonly Guid PsicologoId = Guid.NewGuid();

    private static Contrato ContratoBase(FrequenciaContrato freq = FrequenciaContrato.Semanal) => new()
    {
        Id = ContratoId, ClinicaId = ClinicaId,
        PacienteId = PacienteId, PsicologoId = PsicologoId,
        Status = StatusContrato.Ativo,
        DiaSemanasessao = DiaSemana.Segunda,
        HorarioSessao = new TimeOnly(14, 0),
        DuracaoMinutos = 50,
        Frequencia = freq,
        DataInicio = DateOnly.FromDateTime(DateTime.Today),
        Paciente = new Paciente { Id = PacienteId, Nome = "Maria", ClinicaId = ClinicaId },
        Psicologo = new Psicologo { Id = PsicologoId, Nome = "Dr. João", Crp = "06/1234", ClinicaId = ClinicaId },
    };

    private static (IAppDbContext ctx, ITenantProvider tp) SetupContext(
        Contrato? contrato = null, List<Sessao>? sessoesExistentes = null)
    {
        contrato ??= ContratoBase();
        sessoesExistentes ??= new List<Sessao>();

        var contratos = MockDbSetHelper.CreateMockDbSet(new List<Contrato> { contrato }.AsQueryable());
        var sessoes = MockDbSetHelper.CreateMockDbSet(sessoesExistentes.AsQueryable());

        var ctx = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(ClinicaId);

        ctx.Contratos.Returns(contratos);
        ctx.Sessoes.Returns(sessoes);
        ctx.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        return (ctx, tp);
    }

    [Fact]
    public async Task Handle_ContratoPorMes_GeraQuatroSessoesSemananal()
    {
        var (ctx, tp) = SetupContext();
        var handler = new GerarSessoesRecorrentesCommandHandler(ctx, tp);
        var inicio = ObterProximaSegunda();
        var fim = inicio.AddDays(28);

        var cmd = new GerarSessoesRecorrentesCommand(ContratoId, inicio, fim, null);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().HaveCount(5); // 5 segundas dentro de 4 semanas
        result.Should().OnlyContain(s => s.Status == "Agendada");
        result.Should().OnlyContain(s => s.PacienteNome == "Maria");
    }

    [Fact]
    public async Task Handle_FrequenciaQuinzenal_GeraMetadeDasSessoes()
    {
        var contrato = ContratoBase(FrequenciaContrato.Quinzenal);
        var (ctx, tp) = SetupContext(contrato);
        var handler = new GerarSessoesRecorrentesCommandHandler(ctx, tp);
        var inicio = ObterProximaSegunda();
        var fim = inicio.AddDays(56);

        var cmd = new GerarSessoesRecorrentesCommand(ContratoId, inicio, fim, null);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Count.Should().BeLessThan(5); // menos que semanal
        result.Should().OnlyContain(s => s.Status == "Agendada");
    }

    [Fact]
    public async Task Handle_LimiteQuantidade_RespeituaLimite()
    {
        var (ctx, tp) = SetupContext();
        var handler = new GerarSessoesRecorrentesCommandHandler(ctx, tp);
        var inicio = ObterProximaSegunda();

        var cmd = new GerarSessoesRecorrentesCommand(ContratoId, inicio, null, 3);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_DataJaExistente_NaoDuplica()
    {
        var inicio = ObterProximaSegunda();
        var sessaoExistente = new Sessao
        {
            Id = Guid.NewGuid(), ClinicaId = ClinicaId,
            ContratoId = ContratoId, PacienteId = PacienteId, PsicologoId = PsicologoId,
            Data = inicio, Status = StatusSessao.Agendada,
        };
        var (ctx, tp) = SetupContext(sessoesExistentes: new List<Sessao> { sessaoExistente });
        var handler = new GerarSessoesRecorrentesCommandHandler(ctx, tp);

        var cmd = new GerarSessoesRecorrentesCommand(ContratoId, inicio, inicio.AddDays(7), null);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().NotContain(s => s.Data == inicio);
    }

    [Fact]
    public async Task Handle_ContratoEncerrado_LancaInvalidOperation()
    {
        var contrato = ContratoBase();
        contrato.Status = StatusContrato.Encerrado;
        var (ctx, tp) = SetupContext(contrato);
        var handler = new GerarSessoesRecorrentesCommandHandler(ctx, tp);

        var cmd = new GerarSessoesRecorrentesCommand(ContratoId, DateOnly.FromDateTime(DateTime.Today), null, 4);
        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static DateOnly ObterProximaSegunda()
    {
        var data = DateOnly.FromDateTime(DateTime.Today);
        while (data.DayOfWeek != DayOfWeek.Monday)
            data = data.AddDays(1);
        return data;
    }
}
