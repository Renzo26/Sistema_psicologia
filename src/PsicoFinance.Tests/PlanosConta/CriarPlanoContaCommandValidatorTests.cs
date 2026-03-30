using FluentAssertions;
using PsicoFinance.Application.Features.PlanosConta.Commands.CriarPlanoConta;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Tests.PlanosConta;

public class CriarPlanoContaCommandValidatorTests
{
    private readonly CriarPlanoContaCommandValidator _validator = new();

    [Fact]
    public async Task Validate_CommandValido_Passa()
    {
        var cmd = new CriarPlanoContaCommand("Sessões", TipoPlanoConta.Receita, "Desc");
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_NomeVazio_Falha()
    {
        var cmd = new CriarPlanoContaCommand("", TipoPlanoConta.Receita, null);
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Nome));
    }

    [Fact]
    public async Task Validate_NomeMuitoLongo_Falha()
    {
        var cmd = new CriarPlanoContaCommand(new string('A', 101), TipoPlanoConta.Receita, null);
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_DescricaoMuitoLonga_Falha()
    {
        var cmd = new CriarPlanoContaCommand("Nome", TipoPlanoConta.Receita, new string('A', 301));
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_DescricaoNula_Passa()
    {
        var cmd = new CriarPlanoContaCommand("Nome", TipoPlanoConta.Despesa, null);
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }
}
