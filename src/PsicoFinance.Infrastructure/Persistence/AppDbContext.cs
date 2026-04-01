using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Clinica> Clinicas => Set<Clinica>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Psicologo> Psicologos => Set<Psicologo>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<PlanoConta> PlanosConta => Set<PlanoConta>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<Sessao> Sessoes => Set<Sessao>();
    public DbSet<LancamentoFinanceiro> LancamentosFinanceiros => Set<LancamentoFinanceiro>();
    public DbSet<Repasse> Repasses => Set<Repasse>();
    public DbSet<FechamentoMensal> FechamentosMensais => Set<FechamentoMensal>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Recibo> Recibos => Set<Recibo>();
    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas as configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── snake_case naming convention ─────────────────────────
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Tabela
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            // Colunas
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.GetColumnName()));
            }

            // Chaves
            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()!));
            }

            // Foreign keys
            foreach (var fk in entity.GetForeignKeys())
            {
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName()!));
            }

            // Índices
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
            }
        }

        // ── Filtro global: soft delete ───────────────────────────
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var prop = Expression.Property(parameter, nameof(BaseEntity.ExcluidoEm));
            var condition = Expression.Equal(prop, Expression.Constant(null, typeof(DateTimeOffset?)));

            // Se for TenantEntity, adiciona filtro de clinica_id
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var clinicaIdProp = Expression.Property(parameter, nameof(TenantEntity.ClinicaId));

                // Captura o tenant provider para o filtro
                var tenantProvider = _tenantProvider;
                var clinicaIdValue = Expression.Property(
                    Expression.Constant(tenantProvider),
                    nameof(ITenantProvider.ClinicaId));

                var tenantCondition = Expression.Equal(
                    clinicaIdProp,
                    Expression.Convert(clinicaIdValue, typeof(Guid)));

                var hasValue = Expression.Property(
                    Expression.Constant(tenantProvider),
                    nameof(ITenantProvider.ClinicaId));

                var hasValueCheck = Expression.NotEqual(
                    Expression.Convert(hasValue, typeof(object)),
                    Expression.Constant(null, typeof(object)));

                // Se tenantProvider tem valor, aplica filtro; caso contrário, retorna true
                var tenantFilter = Expression.Condition(
                    hasValueCheck,
                    tenantCondition,
                    Expression.Constant(true));

                condition = Expression.AndAlso(condition, tenantFilter);
            }

            var lambda = Expression.Lambda(condition, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }

        // ── TIMESTAMPTZ para todos os DateTimeOffset ──────────────
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetColumnType("timestamptz");
                }
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    private void ApplyAuditInfo()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CriadoEm = DateTimeOffset.UtcNow;
                    entry.Entity.AtualizadoEm = DateTimeOffset.UtcNow;
                    // Preenche ClinicaId automaticamente para TenantEntity
                    if (entry.Entity is TenantEntity tenantEntity
                        && tenantEntity.ClinicaId == Guid.Empty
                        && _tenantProvider.ClinicaId.HasValue)
                    {
                        tenantEntity.ClinicaId = _tenantProvider.ClinicaId.Value;
                    }
                    break;
                case EntityState.Modified:
                    entry.Entity.AtualizadoEm = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Deleted:
                    // Converte DELETE em soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.ExcluidoEm = DateTimeOffset.UtcNow;
                    break;
            }
        }
    }

    private static string ToSnakeCase(string name)
    {
        return string.Concat(
            name.Select((c, i) =>
                i > 0 && char.IsUpper(c) && !char.IsUpper(name[i - 1])
                    ? $"_{c}"
                    : c.ToString()))
            .ToLower();
    }
}
