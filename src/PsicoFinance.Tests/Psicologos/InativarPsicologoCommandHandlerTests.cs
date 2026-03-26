using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Psicologos.Commands.InativarPsicologo;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Psicologos;

public class InativarPsicologoCommandHandlerTests
{
    [Fact]
    public async Task Handle_Existente_InativaPsicologo()
    {
        var psicologoId = Guid.NewGuid();
        var psicologo = new Psicologo
        {
            Id = psicologoId, ClinicaId = Guid.NewGuid(), Nome = "Dr. João", Crp = "06/12345",
            Tipo = TipoPsicologo.Pj, TipoRepasse = TipoRepasse.Percentual, ValorRepasse = 40, Ativo = true
        };

        var context = Substitute.For<IAppDbContext>();
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Psicologo> { psicologo }.AsQueryable());
        context.Psicologos.Returns(mockSet);
        context.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new InativarPsicologoCommandHandler(context);
        await handler.Handle(new InativarPsicologoCommand(psicologoId), CancellationToken.None);

        psicologo.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NaoExiste_LancaKeyNotFound()
    {
        var context = Substitute.For<IAppDbContext>();
        var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Psicologo>().AsQueryable());
        context.Psicologos.Returns(mockSet);

        var handler = new InativarPsicologoCommandHandler(context);
        var act = () => handler.Handle(new InativarPsicologoCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
