using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Auth.Commands.Login;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _context = Substitute.For<IAppDbContext>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtService = Substitute.For<IJwtService>();
        _handler = new LoginCommandHandler(_context, _passwordHasher, _jwtService);
    }

    [Fact]
    public async Task Handle_CredenciaisValidas_RetornaAuthResponse()
    {
        // Arrange
        var clinica = new Clinica { Id = Guid.NewGuid(), Nome = "Clínica Teste", Ativo = true };
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Dr. Teste",
            Email = "teste@email.com",
            SenhaHash = "hashed_password",
            Role = RoleUsuario.Admin,
            ClinicaId = clinica.Id,
            Clinica = clinica,
            Ativo = true
        };

        var usuarios = new List<Usuario> { usuario }.AsQueryable();
        var mockUsuarios = CreateMockDbSet(usuarios);
        _context.Usuarios.Returns(mockUsuarios);

        var refreshTokensList = new List<RefreshToken>();
        var mockRefreshTokens = CreateMockDbSet(refreshTokensList.AsQueryable());
        _context.RefreshTokens.Returns(mockRefreshTokens);

        _passwordHasher.Verify("senha123", "hashed_password").Returns(true);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed_refresh_token");
        _jwtService.GenerateAccessToken(Arg.Any<Usuario>()).Returns("jwt_token_123");
        _jwtService.GenerateRefreshToken().Returns("refresh_token_123");

        var command = new LoginCommand("teste@email.com", "senha123", "127.0.0.1", "Test/1.0");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UsuarioId.Should().Be(usuario.Id);
        result.Nome.Should().Be("Dr. Teste");
        result.Email.Should().Be("teste@email.com");
        result.Role.Should().Be(RoleUsuario.Admin);
        result.AccessToken.Should().Be("jwt_token_123");
        mockRefreshTokens.Received(1).Add(Arg.Any<RefreshToken>());
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailInexistente_LancaUnauthorized()
    {
        // Arrange
        var mockDbSet = CreateMockDbSet(new List<Usuario>().AsQueryable());
        _context.Usuarios.Returns(mockDbSet);

        var command = new LoginCommand("naoexiste@email.com", "senha123", null, null);

        // Act & Assert
        await FluentActions
            .Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Email ou senha inválidos.");
    }

    [Fact]
    public async Task Handle_SenhaIncorreta_LancaUnauthorized()
    {
        // Arrange
        var clinica = new Clinica { Id = Guid.NewGuid(), Nome = "Clínica", Ativo = true };
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "teste@email.com",
            SenhaHash = "hashed",
            Clinica = clinica,
            ClinicaId = clinica.Id,
            Ativo = true
        };

        var mockDbSet = CreateMockDbSet(new List<Usuario> { usuario }.AsQueryable());
        _context.Usuarios.Returns(mockDbSet);
        _passwordHasher.Verify("senhaerrada", "hashed").Returns(false);

        var command = new LoginCommand("teste@email.com", "senhaerrada", null, null);

        // Act & Assert
        await FluentActions
            .Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ClinicaInativa_LancaUnauthorized()
    {
        // Arrange
        var clinica = new Clinica { Id = Guid.NewGuid(), Nome = "Clínica Inativa", Ativo = false };
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "teste@email.com",
            SenhaHash = "hashed",
            Clinica = clinica,
            ClinicaId = clinica.Id,
            Ativo = true
        };

        var mockDbSet = CreateMockDbSet(new List<Usuario> { usuario }.AsQueryable());
        _context.Usuarios.Returns(mockDbSet);
        _passwordHasher.Verify("senha123", "hashed").Returns(true);

        var command = new LoginCommand("teste@email.com", "senha123", null, null);

        // Act & Assert
        await FluentActions
            .Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Clínica inativa*");
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

// Helpers para async EF Core queries com NSubstitute
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;
    internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(System.Linq.Expressions.Expression expression)
        => _inner.Execute(expression);

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(System.Linq.Expressions.Expression)])!
            .MakeGenericMethod(resultType)
            .Invoke(_inner, [expression]);

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, [executionResult])!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public T Current => _inner.Current;
    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
    public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
}
