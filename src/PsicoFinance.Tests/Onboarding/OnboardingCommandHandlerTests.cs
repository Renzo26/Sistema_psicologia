using FluentAssertions;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Onboarding.Commands;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Tests.Common;

namespace PsicoFinance.Tests.Onboarding;

public class OnboardingCommandHandlerTests
{
    private readonly IAppDbContext _context;
    private readonly ITenantSchemaService _schemaService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IAuditService _auditService;
    private readonly OnboardingCommandHandler _handler;

    public OnboardingCommandHandlerTests()
    {
        _context = Substitute.For<IAppDbContext>();
        _schemaService = Substitute.For<ITenantSchemaService>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtService = Substitute.For<IJwtService>();
        _auditService = Substitute.For<IAuditService>();
        _handler = new OnboardingCommandHandler(_context, _schemaService, _passwordHasher, _jwtService, _auditService);
    }

    [Fact]
    public async Task Handle_DadosValidos_CriaClinicaEUsuarioAdmin()
    {
        // Arrange
        var mockClinicas = MockDbSetHelper.CreateMockDbSet(new List<Clinica>().AsQueryable());
        var mockUsuarios = MockDbSetHelper.CreateMockDbSet(new List<Usuario>().AsQueryable());
        _context.Clinicas.Returns(mockClinicas);
        _context.Usuarios.Returns(mockUsuarios);
        _passwordHasher.Hash("Senha123").Returns("hashed_senha");
        _jwtService.GenerateAccessToken(Arg.Any<Usuario>()).Returns("jwt_token_123");

        var command = new OnboardingCommand(
            NomeClinica: "Clínica Teste",
            Cnpj: "12.345.678/0001-00",
            EmailClinica: "contato@clinicateste.com",
            Telefone: "(11) 99999-0000",
            NomeAdmin: "Admin Teste",
            EmailAdmin: "admin@clinicateste.com",
            SenhaAdmin: "Senha123",
            IpOrigem: "127.0.0.1",
            UserAgent: "TestAgent");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.NomeClinica.Should().Be("Clínica Teste");
        result.NomeUsuario.Should().Be("Admin Teste");
        result.Email.Should().Be("admin@clinicateste.com");
        result.AccessToken.Should().Be("jwt_token_123");
        result.ClinicaId.Should().NotBeEmpty();
        result.UsuarioId.Should().NotBeEmpty();

        mockClinicas.Received(1).Add(Arg.Is<Clinica>(c =>
            c.Nome == "Clínica Teste" &&
            c.Cnpj == "12.345.678/0001-00" &&
            c.Email == "contato@clinicateste.com"));

        mockUsuarios.Received(1).Add(Arg.Is<Usuario>(u =>
            u.Nome == "Admin Teste" &&
            u.Email == "admin@clinicateste.com" &&
            u.Role == Domain.Enums.RoleUsuario.Admin));

        await _schemaService.Received(1).CreateSchemaForTenantAsync(
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await _auditService.Received(1).RegistrarAsync(
            Arg.Is<AuditLog>(a =>
                a.Acao == "Criar" &&
                a.Entidade == nameof(Clinica)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailAdminDuplicado_LancaExcecao()
    {
        // Arrange
        var existingUser = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "admin@clinicateste.com",
            Nome = "Existente",
            SenhaHash = "hash",
            ClinicaId = Guid.NewGuid()
        };

        var mockClinicas = MockDbSetHelper.CreateMockDbSet(new List<Clinica>().AsQueryable());
        var mockUsuarios = MockDbSetHelper.CreateMockDbSet(new List<Usuario> { existingUser }.AsQueryable());
        _context.Clinicas.Returns(mockClinicas);
        _context.Usuarios.Returns(mockUsuarios);

        var command = new OnboardingCommand(
            NomeClinica: "Nova Clínica",
            Cnpj: null,
            EmailClinica: "nova@clinica.com",
            Telefone: null,
            NomeAdmin: "Admin",
            EmailAdmin: "admin@clinicateste.com",
            SenhaAdmin: "Senha123",
            IpOrigem: null,
            UserAgent: null);

        // Act & Assert
        await FluentActions
            .Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task Handle_CnpjDuplicado_LancaExcecao()
    {
        // Arrange
        var existingClinica = new Clinica
        {
            Id = Guid.NewGuid(),
            Nome = "Clínica Existente",
            Cnpj = "12.345.678/0001-00",
            Email = "existente@clinica.com"
        };

        var mockClinicas = MockDbSetHelper.CreateMockDbSet(new List<Clinica> { existingClinica }.AsQueryable());
        var mockUsuarios = MockDbSetHelper.CreateMockDbSet(new List<Usuario>().AsQueryable());
        _context.Clinicas.Returns(mockClinicas);
        _context.Usuarios.Returns(mockUsuarios);

        var command = new OnboardingCommand(
            NomeClinica: "Nova Clínica",
            Cnpj: "12.345.678/0001-00",
            EmailClinica: "nova@clinica.com",
            Telefone: null,
            NomeAdmin: "Admin",
            EmailAdmin: "admin@nova.com",
            SenhaAdmin: "Senha123",
            IpOrigem: null,
            UserAgent: null);

        // Act & Assert
        await FluentActions
            .Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*CNPJ*");
    }
}
