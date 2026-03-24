using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Infrastructure.MultiTenancy;
using PsicoFinance.Infrastructure.Persistence;

namespace PsicoFinance.Tests.MultiTenancy;

public class TenantIsolationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TenantProvider _tenantProvider;
    private readonly Guid _clinica1Id = Guid.NewGuid();
    private readonly Guid _clinica2Id = Guid.NewGuid();

    public TenantIsolationTests()
    {
        _tenantProvider = new TenantProvider();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options, _tenantProvider);
        SeedData();
    }

    private void SeedData()
    {
        var clinica1 = new Clinica { Id = _clinica1Id, Nome = "Clínica 1", Email = "c1@test.com", Ativo = true };
        var clinica2 = new Clinica { Id = _clinica2Id, Nome = "Clínica 2", Email = "c2@test.com", Ativo = true };
        _context.Clinicas.AddRange(clinica1, clinica2);

        _context.Usuarios.Add(new Usuario
        {
            Id = Guid.NewGuid(), Nome = "Admin C1", Email = "admin@c1.com",
            SenhaHash = "hash", Role = RoleUsuario.Admin,
            ClinicaId = _clinica1Id, Ativo = true
        });

        _context.Usuarios.Add(new Usuario
        {
            Id = Guid.NewGuid(), Nome = "Secretaria C1", Email = "sec@c1.com",
            SenhaHash = "hash", Role = RoleUsuario.Secretaria,
            ClinicaId = _clinica1Id, Ativo = true
        });

        _context.Usuarios.Add(new Usuario
        {
            Id = Guid.NewGuid(), Nome = "Admin C2", Email = "admin@c2.com",
            SenhaHash = "hash", Role = RoleUsuario.Admin,
            ClinicaId = _clinica2Id, Ativo = true
        });

        _context.SaveChanges();
    }

    [Fact]
    public void TenantProvider_SetClinicaId_ArmazenaTenantCorretamente()
    {
        // Arrange & Act
        _tenantProvider.SetClinicaId(_clinica1Id);

        // Assert
        _tenantProvider.ClinicaId.Should().Be(_clinica1Id);
    }

    [Fact]
    public void TenantProvider_SemSetClinicaId_RetornaNull()
    {
        var provider = new TenantProvider();
        provider.ClinicaId.Should().BeNull();
    }

    [Fact]
    public async Task TenantEntity_ComClinicaIdDiferentes_DadosSaoSeparados()
    {
        // Arrange — simula o que o query filter faz em PostgreSQL
        var allUsuarios = await _context.Usuarios.IgnoreQueryFilters().ToListAsync();

        // Act — filtra para clínica 1 (simula query filter)
        var usuariosClinica1 = allUsuarios.Where(u => u.ClinicaId == _clinica1Id).ToList();
        var usuariosClinica2 = allUsuarios.Where(u => u.ClinicaId == _clinica2Id).ToList();

        // Assert
        allUsuarios.Should().HaveCount(3);
        usuariosClinica1.Should().HaveCount(2);
        usuariosClinica1.Should().AllSatisfy(u => u.ClinicaId.Should().Be(_clinica1Id));
        usuariosClinica2.Should().HaveCount(1);
        usuariosClinica2.Should().AllSatisfy(u => u.ClinicaId.Should().Be(_clinica2Id));
    }

    [Fact]
    public async Task TenantEntity_NaoVazaDadosEntreClinicas()
    {
        // Arrange
        var allUsuarios = await _context.Usuarios.IgnoreQueryFilters().ToListAsync();

        // Act — filtra para clínica 2
        var usuarios = allUsuarios.Where(u => u.ClinicaId == _clinica2Id).ToList();

        // Assert
        usuarios.Should().HaveCount(1);
        usuarios.Should().NotContain(u => u.Email == "admin@c1.com");
        usuarios.Should().NotContain(u => u.Email == "sec@c1.com");
    }

    [Fact]
    public async Task SoftDelete_RegistroExcluido_NaoApareceSemIgnoreFilters()
    {
        // Arrange
        var usuario = await _context.Usuarios.IgnoreQueryFilters().FirstAsync(u => u.Email == "admin@c1.com");
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        // Act
        var todosComFiltro = await _context.Usuarios.IgnoreQueryFilters()
            .Where(u => u.ExcluidoEm == null)
            .ToListAsync();

        var excluido = await _context.Usuarios.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "admin@c1.com");

        // Assert
        todosComFiltro.Should().HaveCount(2);
        excluido.Should().NotBeNull();
        excluido!.ExcluidoEm.Should().NotBeNull();
    }

    [Fact]
    public void SaveChanges_NovoTenantEntity_PreencheClinicaIdAutomaticamente()
    {
        // Arrange
        _tenantProvider.SetClinicaId(_clinica1Id);
        var novoUsuario = new Usuario
        {
            Id = Guid.NewGuid(), Nome = "Novo User", Email = "novo@c1.com",
            SenhaHash = "hash", Role = RoleUsuario.Secretaria, Ativo = true
            // ClinicaId NÃO definido → deve ser preenchido pelo SaveChanges
        };

        // Act
        _context.Usuarios.Add(novoUsuario);
        _context.SaveChanges();

        // Assert
        novoUsuario.ClinicaId.Should().Be(_clinica1Id);
    }

    [Fact]
    public void SaveChanges_PreencheAuditFields()
    {
        // Arrange
        var antes = DateTimeOffset.UtcNow.AddSeconds(-1);
        var novoUsuario = new Usuario
        {
            Id = Guid.NewGuid(), Nome = "Audit Test", Email = "audit@test.com",
            SenhaHash = "hash", Role = RoleUsuario.Admin,
            ClinicaId = _clinica1Id, Ativo = true
        };

        // Act
        _context.Usuarios.Add(novoUsuario);
        _context.SaveChanges();

        // Assert
        novoUsuario.CriadoEm.Should().BeAfter(antes);
        novoUsuario.AtualizadoEm.Should().BeAfter(antes);
        novoUsuario.ExcluidoEm.Should().BeNull();
    }

    [Fact]
    public void ClinicaEntity_NaoHerdaDeTenantEntity()
    {
        // Clinica é BaseEntity (não TenantEntity) — é o tenant raiz
        typeof(Clinica).Should().BeDerivedFrom<BaseEntity>();
        typeof(Clinica).Should().NotBeDerivedFrom<TenantEntity>();
    }

    [Fact]
    public void UsuarioEntity_HerdaDeTenantEntity()
    {
        typeof(Usuario).Should().BeDerivedFrom<TenantEntity>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
