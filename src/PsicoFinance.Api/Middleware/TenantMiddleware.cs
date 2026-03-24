using System.Security.Claims;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Infrastructure.MultiTenancy;

namespace PsicoFinance.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        // 1. Tenta extrair do JWT (claim "clinica_id")
        var clinicaIdClaim = context.User.FindFirstValue("clinica_id");
        if (!string.IsNullOrEmpty(clinicaIdClaim) && Guid.TryParse(clinicaIdClaim, out var clinicaId))
        {
            tenantProvider.SetClinicaId(clinicaId);
            await _next(context);
            return;
        }

        // 2. Tenta extrair do subdomínio (ex: clinica1.psicofinance.com.br)
        var host = context.Request.Host.Host;
        var parts = host.Split('.');
        if (parts.Length > 2)
        {
            var subdomain = parts[0];
            if (context.RequestServices.GetService<ITenantResolver>() is { } resolver)
            {
                var resolved = await resolver.ResolveBySubdomainAsync(subdomain);
                if (resolved.HasValue)
                {
                    tenantProvider.SetClinicaId(resolved.Value);
                }
            }
        }

        await _next(context);
    }
}
