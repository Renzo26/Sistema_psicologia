using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Auth.Commands.Register;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _context = Substitute.For<IAppDbContext>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _handler = new RegisterCommandHandler(_context, _passwordHasher);
    }

    [Fact]
    public async Task Handle_DadosValidos_CriaUsuario()
    {
        // Arrange
        var clinicaId = Guid.NewGuid();
        var mockDbSet = CreateMockDbSet(new List<Usuario>().AsQueryable());
        _context.Usuarios.Returns(mockDbSet);
        _passwordHasher.Hash("Senha123!").Returns("hashed_password");

        var command = new RegisterCommand(
            Nome: "Dr. Teste",
            Email: "novo@email.com",
            Senha: "Senha123!",
            Role: RoleUsuario.Psicologo,
            ClinicaId: clinicaId,
            PsicologoId: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Dr. Teste");
        result.Email.Should().Be("novo@email.com");
        result.Role.Should().Be(RoleUsuario.Psicologo);
        result.ClinicaId.Should().Be(clinicaId);
        result.Ativo.Should().BeTrue();
        _context.Usuarios.Received(1).Add(Arg.Any<Usuario>());
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailDuplicado_LancaArgumentException()
    {
        // Arrange
        var clinicaId = Guid.NewGuid();
        var existente = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "existente@email.com",
            ClinicaId = clinicaId,
            Ativo = true
        };

        var mockDbSet = CreateMockDbSet(new List<Usuario> { existente }.AsQueryable());
        _context.Usuarios.Returns(mockDbSet);

        var command = new RegisterCommand(
            Nome: "Outro",
            Email: "existente@email.com",
            Senha: "Senha123!",
            Role: RoleUsuario.Secretaria,
            ClinicaId: clinicaId,
            PsicologoId: null
        );

        // Act & Assert
        await FluentActions
            .Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*email*");
    }

    private static DbSet<T> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>();

        ((IQueryable<T>)mockSet).Provider.Returns(new TestAsyncQueryProvider<T>(data.Provider));
        ((IQueryable<T>)mockSet).Expression.Returns(data.Expression);
        ((IQueryable<T>)mockSet).ElementType.Returns(data.ElementType);
        ((IQueryable<T>)mockSet).GetEnumerator().Returns(data.GetEnumerator());
        ((IAsyncEnumerable<T>)mockSet).GetAsyncEnumerator(Arg.Any<CancellationToken>())
            .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

        return mockSet;
    }
}
