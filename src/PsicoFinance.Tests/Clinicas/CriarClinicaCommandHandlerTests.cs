using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Clinicas.Commands.CriarClinica;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Infrastructure.MultiTenancy;
using PsicoFinance.Infrastructure.Persistence;

namespace PsicoFinance.Tests.Clinicas;

public class CriarClinicaCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TenantProvider _tenantProvider;
    private readonly ITenantSchemaService _schemaService;
    private readonly IAuditService _auditService;

    public CriarClinicaCommandHandlerTests()
    {
        _tenantProvider = new TenantProvider();
        _schemaService = Substitute.For<ITenantSchemaService>();
        _auditService = Substitute.For<IAuditService>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options, _tenantProvider);
    }

    private CriarClinicaCommand CriarCommand(
        string? cnpj = "12.345.678/0001-90",
        string email = "nova@clinica.com") => new(
        Nome: "Clínica Nova",
        Cnpj: cnpj,
        Email: email,
        Telefone: "(11) 99999-0000",
        Cep: "01310-100",
        Logradouro: "Av. Paulista",
        Numero: "1000",
        Complemento: "Sala 501",
        Bairro: "Bela Vista",
        Cidade: "São Paulo",
        Estado: "SP");

    private CriarClinicaCommandHandler CriarHandler() =>
        new(_context, _schemaService, _tenantProvider, _auditService);

    [Fact]
    public async Task Handle_DadosValidos_CriaClinica()
    {
        var handler = CriarHandler();

        var result = await handler.Handle(CriarCommand(), CancellationToken.None);

        result.Nome.Should().Be("Clínica Nova");
        result.Email.Should().Be("nova@clinica.com");
        result.Cnpj.Should().Be("12.345.678/0001-90");
        result.Cidade.Should().Be("São Paulo");
        result.Estado.Should().Be("SP");
        result.Ativo.Should().BeTrue();
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_DadosValidos_PersistNoBanco()
    {
        var handler = CriarHandler();

        var result = await handler.Handle(CriarCommand(), CancellationToken.None);

        var clinica = await _context.Clinicas.IgnoreQueryFilters()
            .FirstAsync(c => c.Id == result.Id);
        clinica.Nome.Should().Be("Clínica Nova");
        clinica.Email.Should().Be("nova@clinica.com");
    }

    [Fact]
    public async Task Handle_DadosValidos_CriaSchemaTenant()
    {
        var handler = CriarHandler();

        var result = await handler.Handle(CriarCommand(), CancellationToken.None);

        await _schemaService.Received(1)
            .CreateSchemaForTenantAsync(result.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DadosValidos_RegistraAuditoria()
    {
        var handler = CriarHandler();

        await handler.Handle(CriarCommand(), CancellationToken.None);

        await _auditService.Received(1).RegistrarAsync(
            Arg.Is<AuditLog>(a => a.Acao == "Criar" && a.Entidade == "Clinica"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CnpjDuplicado_LancaInvalidOperation()
    {
        _context.Clinicas.Add(new Clinica
        {
            Id = Guid.NewGuid(),
            Nome = "Existente",
            Cnpj = "12.345.678/0001-90",
            Email = "existente@clinica.com",
            Ativo = true
        });
        await _context.SaveChangesAsync();

        var handler = CriarHandler();

        var act = () => handler.Handle(CriarCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CNPJ*");
    }

    [Fact]
    public async Task Handle_EmailDuplicado_LancaInvalidOperation()
    {
        _context.Clinicas.Add(new Clinica
        {
            Id = Guid.NewGuid(),
            Nome = "Existente",
            Email = "nova@clinica.com",
            Ativo = true
        });
        await _context.SaveChangesAsync();

        var handler = CriarHandler();

        var act = () => handler.Handle(CriarCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task Handle_CnpjNulo_CriaSemProblema()
    {
        var handler = CriarHandler();

        var result = await handler.Handle(CriarCommand(cnpj: null), CancellationToken.None);

        result.Cnpj.Should().BeNull();
        result.Nome.Should().Be("Clínica Nova");
    }

    public void Dispose() => _context.Dispose();
}
