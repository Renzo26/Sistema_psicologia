using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Psicologos.Commands.AtualizarPsicologo;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Psicologos;

public class AtualizarPsicologoCommandHandlerTests
{
    private readonly Guid _psicologoId = Guid.NewGuid();
    private readonly Guid _clinicaId = Guid.NewGuid();

    private AtualizarPsicologoCommand Cmd() => new(
        _psicologoId, "Atualizado", "06/22222", "a@b.com", null, null,
        TipoPsicologo.Pj, TipoRepasse.Percentual, 50, null, null, null, null);

    [Fact]
    public async Task Handle_DadosValidos_Atualiza()
    {
        var psicologo = new Psicologo
        {
            Id = _psicologoId, ClinicaId = _clinicaId, Nome = "Original", Crp = "06/11111",
            Tipo = TipoPsicologo.Clt, TipoRepasse = TipoRepasse.ValorFixo, ValorRepasse = 100, Ativo = true
        };

        var context = Substitute.For<IAppDbContext>();
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Psicologo> { psicologo }.AsQueryable());
        context.Psicologos.Returns(mockSet);
        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new AtualizarPsicologoCommandHandler(context);
        var result = await handler.Handle(Cmd(), CancellationToken.None);

        result.Nome.Should().Be("Atualizado");
        result.Crp.Should().Be("06/22222");
        result.Tipo.Should().Be(TipoPsicologo.Pj);
    }

    [Fact]
    public async Task Handle_NaoExiste_LancaKeyNotFound()
    {
        var context = Substitute.For<IAppDbContext>();
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Psicologo>().AsQueryable());
        context.Psicologos.Returns(mockSet);

        var handler = new AtualizarPsicologoCommandHandler(context);
        var act = () => handler.Handle(Cmd(), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
