using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Psicologos.Commands.CriarPsicologo;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Psicologos;

public class CriarPsicologoCommandValidatorIntegrationTests
{
    [Fact]
    public void Valido_SemErros()
    {
        var validator = new CriarPsicologoCommandValidator();
        var cmd = new CriarPsicologoCommand(
            Nome: "Dr. João", Crp: "06/12345", Email: "joao@psi.com",
            Telefone: "(11) 99999-0000", Cpf: "123.456.789-00",
            Tipo: TipoPsicologo.Pj, TipoRepasse: TipoRepasse.Percentual, ValorRepasse: 40,
            Banco: null, Agencia: null, Conta: null, PixChave: null);

        var result = validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }
}

public class CriarPsicologoCommandHandlerTests
{
    private static CriarPsicologoCommand Cmd(string crp = "06/12345") => new(
        Nome: "Dr. João", Crp: crp, Email: "joao@psi.com", Telefone: null, Cpf: null,
        Tipo: TipoPsicologo.Pj, TipoRepasse: TipoRepasse.Percentual, ValorRepasse: 40,
        Banco: null, Agencia: null, Conta: null, PixChave: "joao@pix.com");

    [Fact]
    public async Task Handle_DadosValidos_CriaPsicologo()
    {
        var clinicaId = Guid.NewGuid();
        var context = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(clinicaId);

        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Psicologo>().AsQueryable());
        context.Psicologos.Returns(mockSet);
        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new CriarPsicologoCommandHandler(context, tp);
        var result = await handler.Handle(Cmd(), CancellationToken.None);

        result.Nome.Should().Be("Dr. João");
        result.Crp.Should().Be("06/12345");
        result.Tipo.Should().Be(TipoPsicologo.Pj);
        result.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CrpDuplicado_Lanca()
    {
        var clinicaId = Guid.NewGuid();
        var context = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns(clinicaId);

        var existente = new Psicologo
        {
            Id = Guid.NewGuid(), ClinicaId = clinicaId, Nome = "Existente", Crp = "06/12345",
            Tipo = TipoPsicologo.Pj, TipoRepasse = TipoRepasse.Percentual, ValorRepasse = 40
        };
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Psicologo> { existente }.AsQueryable());
        context.Psicologos.Returns(mockSet);

        var handler = new CriarPsicologoCommandHandler(context, tp);
        var act = () => handler.Handle(Cmd(), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*CRP*");
    }

    [Fact]
    public async Task Handle_TenantNulo_LancaUnauthorized()
    {
        var context = Substitute.For<IAppDbContext>();
        var tp = Substitute.For<ITenantProvider>();
        tp.ClinicaId.Returns((Guid?)null);

        var handler = new CriarPsicologoCommandHandler(context, tp);
        var act = () => handler.Handle(Cmd(), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
