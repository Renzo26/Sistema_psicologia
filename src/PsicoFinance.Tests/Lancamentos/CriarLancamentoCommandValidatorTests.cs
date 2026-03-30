using FluentAssertions;
using PsicoFinance.Application.Features.Lancamentos.Commands.CriarLancamento;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Tests.Lancamentos;

public class CriarLancamentoCommandValidatorTests
{
    private readonly CriarLancamentoCommandValidator _validator = new();

    private static CriarLancamentoCommand Cmd(
        string descricao = "Sessão João",
        decimal valor = 150m,
        string competencia = "2025-03") =>
        new(descricao, valor, TipoLancamento.Receita,
            DateOnly.FromDateTime(DateTime.Today), competencia,
            Guid.NewGuid(), null, null);

    [Fact]
    public async Task Validate_CommandValido_Passa()
    {
        var result = await _validator.ValidateAsync(Cmd());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_DescricaoVazia_Falha()
    {
        var result = await _validator.ValidateAsync(Cmd(descricao: ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CriarLancamentoCommand.Descricao));
    }

    [Fact]
    public async Task Validate_ValorZero_Falha()
    {
        var result = await _validator.ValidateAsync(Cmd(valor: 0));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CriarLancamentoCommand.Valor));
    }

    [Fact]
    public async Task Validate_ValorNegativo_Falha()
    {
        var result = await _validator.ValidateAsync(Cmd(valor: -10m));
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("2025-13")]
    [InlineData("25-03")]
    [InlineData("2025/03")]
    [InlineData("")]
    public async Task Validate_CompetenciaInvalida_Falha(string competencia)
    {
        var result = await _validator.ValidateAsync(Cmd(competencia: competencia));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_PlanoContaVazio_Falha()
    {
        var cmd = new CriarLancamentoCommand("Desc", 100m, TipoLancamento.Receita,
            DateOnly.FromDateTime(DateTime.Today), "2025-03", Guid.Empty, null, null);
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }
}
