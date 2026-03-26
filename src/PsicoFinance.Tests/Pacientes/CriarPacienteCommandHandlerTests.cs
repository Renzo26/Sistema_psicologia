using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Pacientes.Commands.CriarPaciente;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Pacientes;

public class CriarPacienteCommandHandlerTests
{
    private static CriarPacienteCommand Cmd(string? cpf = "123.456.789-00") => new(
        Nome: "Maria Silva", Cpf: cpf, Email: "maria@email.com",
        Telefone: null, DataNascimento: new DateOnly(1990, 5, 15),
        ResponsavelNome: null, ResponsavelTelefone: null, Observacoes: null);

    [Fact]
    public async Task Handle_DadosValidos_CriaPaciente()
    {
        var clinicaId = Guid.NewGuid();
        var context = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(clinicaId);

        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Paciente>().AsQueryable());
        context.Pacientes.Returns(mockSet);
        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new CriarPacienteCommandHandler(context, tp);
        var result = await handler.Handle(Cmd(), CancellationToken.None);

        result.Nome.Should().Be("Maria Silva");
        result.Cpf.Should().Be("123.456.789-00");
        result.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CpfDuplicado_Lanca()
    {
        var clinicaId = Guid.NewGuid();
        var context = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(clinicaId);

        var existente = new Paciente
        {
            Id = Guid.NewGuid(), ClinicaId = clinicaId, Nome = "Existente",
            Cpf = "123.456.789-00", Ativo = true
        };
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Paciente> { existente }.AsQueryable());
        context.Pacientes.Returns(mockSet);

        var handler = new CriarPacienteCommandHandler(context, tp);
        var act = () => handler.Handle(Cmd(), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*CPF*");
    }

    [Fact]
    public async Task Handle_CpfNulo_NaoVerificaDuplicidade()
    {
        var clinicaId = Guid.NewGuid();
        var context = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(clinicaId);

        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Paciente>().AsQueryable());
        context.Pacientes.Returns(mockSet);
        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new CriarPacienteCommandHandler(context, tp);
        var result = await handler.Handle(Cmd(cpf: null), CancellationToken.None);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_TenantNulo_LancaUnauthorized()
    {
        var context = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns((Guid?)null);

        var handler = new CriarPacienteCommandHandler(context, tp);
        var act = () => handler.Handle(Cmd(), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
