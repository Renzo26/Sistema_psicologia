namespace PsicoFinance.Infrastructure.MultiTenancy;

public interface ITenantResolver
{
    Task<Guid?> ResolveBySubdomainAsync(string subdomain);
}
