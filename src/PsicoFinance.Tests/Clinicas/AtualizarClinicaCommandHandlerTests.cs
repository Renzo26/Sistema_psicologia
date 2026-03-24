using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Clinicas.Commands.AtualizarClinica;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Infrastructure.MultiTenancy;
using PsicoFinance.Infrastructure.Persistence;

namespace PsicoFinance.Tests.Clinicas;

public class AtualizarClinicaCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TenantProvider _tenantProvider;
    private readonly IAuditService _auditService;
    private readonly Guid _clinicaId = Guid.NewGuid();

    public AtualizarClinicaCommandHandlerTests()
    {
        _tenantProvider = new TenantProvider();
        _auditService = Substitute.For<IAuditService>();

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
            Nome = "Clínica Original",
            Email = "original@clinica.com",
            Ativo = true
        });
        _context.SaveChanges();
    }

    private AtualizarClinicaCommand CriarCommand() => new(
        Nome: "Clínica Atualizada",
        Cnpj: "98.765.432/0001-10",
        Email: "novo@clinica.com",
        Telefone: "(21) 88888-0000",
        Cep: "20040-020",
        Logradouro: "Rua da Assembleia",
        Numero: "10",
        Complemento: "14º andar",
        Bairro: "Centro",
        Cidade: "Rio de Janeiro",
        Estado: "RJ",
        HorarioEnvioAlerta: new TimeOnly(9, 30),
        WebhookN8nUrl: "https://n8n.example.com/hook",
        Timezone: "America/Sao_Paulo");

    [Fact]
    public async Task Handle_DadosValidos_AtualizaClinica()
    {
        _tenantProvider.SetClinicaId(_clinicaId);
        var handler = new AtualizarClinicaCommandHandler(_context, _tenantProvider, _auditService);

        var result = await handler.Handle(CriarCommand(), CancellationToken.None);

        result.Nome.Should().Be("Clínica Atualizada");
        result.Email.Should().Be("novo@clinica.com");
        result.Cidade.Should().Be("Rio de Janeiro");
        result.Estado.Should().Be("RJ");

        // Verificar persistência
        var clinica = await _context.Clinicas.IgnoreQueryFilters()
            .FirstAsync(c => c.Id == _clinicaId);
        clinica.Nome.Should().Be("Clínica Atualizada");
    }

    [Fact]
    public async Task Handle_DadosValidos_RegistraAuditoria()
    {
        _tenantProvider.SetClinicaId(_clinicaId);
        var handler = new AtualizarClinicaCommandHandler(_context, _tenantProvider, _auditService);

        await handler.Handle(CriarCommand(), CancellationToken.None);

        await _auditService.Received(1).RegistrarAsync(
            Arg.Is<AuditLog>(a => a.Acao == "Atualizar" && a.Entidade == "Clinica"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TenantNulo_LancaUnauthorized()
    {
        var handler = new AtualizarClinicaCommandHandler(_context, _tenantProvider, _auditService);

        var act = () => handler.Handle(CriarCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ClinicaNaoExiste_LancaKeyNotFound()
    {
        _tenantProvider.SetClinicaId(Guid.NewGuid());
        var handler = new AtualizarClinicaCommandHandler(_context, _tenantProvider, _auditService);

        var act = () => handler.Handle(CriarCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    public void Dispose() => _context.Dispose();
}
