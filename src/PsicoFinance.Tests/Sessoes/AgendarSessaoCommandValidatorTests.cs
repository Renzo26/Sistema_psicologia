using FluentAssertions;
using PsicoFinance.Application.Features.Sessoes.Commands.AgendarSessao;

namespace PsicoFinance.Tests.Sessoes;

public class AgendarSessaoCommandValidatorTests
{
    private readonly AgendarSessaoCommandValidator _validator = new();

    [Fact]
    public void Validate_DadosValidos_SemErros()
    {
        var cmd = new AgendarSessaoCommand(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today),
            null, null, null);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ContratoIdVazio_RetornaErro()
    {
        var cmd = new AgendarSessaoCommand(
            Guid.Empty,
            DateOnly.FromDateTime(DateTime.Today),
            null, null, null);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ContratoId");
    }

    [Fact]
    public void Validate_DataVazia_RetornaErro()
    {
        var cmd = new AgendarSessaoCommand(
            Guid.NewGuid(),
            default,
            null, null, null);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Data");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(0)]
    [InlineData(300)]
    public void Validate_DuracaoForaDoRange_RetornaErro(int duracao)
    {
        var cmd = new AgendarSessaoCommand(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today),
            null, duracao, null);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DuracaoMinutos");
    }

    [Theory]
    [InlineData(15)]
    [InlineData(50)]
    [InlineData(240)]
    public void Validate_DuracaoValida_SemErros(int duracao)
    {
        var cmd = new AgendarSessaoCommand(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today),
            null, duracao, null);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }
}
