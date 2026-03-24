using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Clinicas.Queries.ObterClinica;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Infrastructure.MultiTenancy;
using PsicoFinance.Infrastructure.Persistence;

namespace PsicoFinance.Tests.Clinicas;

public class ObterClinicaQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TenantProvider _tenantProvider;
    private readonly Guid _clinicaId = Guid.NewGuid();

    public ObterClinicaQueryHandlerTests()
    {
        _tenantProvider = new TenantProvider();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options, _tenantProvider);
        SeedData();
    }

    private void SeedData()
    {
        _context.Clinicas.Add(new Clinica
        {
            Id = _clinicaId,
            Nome = "Clínica Teste",
            Email = "teste@clinica.com",
            Cnpj = "12.345.678/0001-90",
            Telefone = "(11) 99999-0000",
            Cidade = "São Paulo",
            Estado = "SP",
            Ativo = true
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_TenantValido_RetornaClinica()
    {
        _tenantProvider.SetClinicaId(_clinicaId);
        var handler = new ObterClinicaQueryHandler(_context, _tenantProvider);

        var result = await handler.Handle(new ObterClinicaQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(_clinicaId);
        result.Nome.Should().Be("Clínica Teste");
        result.Email.Should().Be("teste@clinica.com");
        result.Cidade.Should().Be("São Paulo");
    }

    [Fact]
    public async Task Handle_TenantNulo_LancaUnauthorized()
    {
        var handler = new ObterClinicaQueryHandler(_context, _tenantProvider);

        var act = () => handler.Handle(new ObterClinicaQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ClinicaNaoExiste_LancaKeyNotFound()
    {
        _tenantProvider.SetClinicaId(Guid.NewGuid());
        var handler = new ObterClinicaQueryHandler(_context, _tenantProvider);

        var act = () => handler.Handle(new ObterClinicaQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    public void Dispose() => _context.Dispose();
}
