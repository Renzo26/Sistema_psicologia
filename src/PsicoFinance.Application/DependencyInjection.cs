using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PsicoFinance.Application.Common.Behaviors;
using PsicoFinance.Application.Features.RelatoriosBI.Services;

namespace PsicoFinance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // ── Serviços de suporte da Application ────────────────────
        services.AddSingleton<RelatorioTemplatesService>();

        return services;
    }
}
