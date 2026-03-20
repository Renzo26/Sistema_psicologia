using Microsoft.EntityFrameworkCore;
using PsicoFinance.Infrastructure.Persistence;

namespace PsicoFinance.Infrastructure.MultiTenancy;

public class TenantResolver : ITenantResolver
{
    private readonly AppDbContext _db;

    public TenantResolver(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid?> ResolveBySubdomainAsync(string subdomain)
    {
        var clinica = await _db.Clinicas
            .AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Ativo)
            .FirstOrDefaultAsync(c => c.Cnpj == subdomain || c.Id.ToString() == subdomain);

        return clinica?.Id;
    }
}
