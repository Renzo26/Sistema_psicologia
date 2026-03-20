# PsicoFinance — Sistema de Gestão para Clínicas de Psicologia

SaaS multi-tenant de gestão financeira especializado para clínicas de psicologia no Brasil.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 8 (C#) — Modular Monolith + CQRS |
| Frontend | Angular 18+ (TypeScript) |
| Banco de Dados | PostgreSQL 16 (Supabase) |
| Automações | N8N (webhooks externos) |
| Jobs | Hangfire |
| Auth | JWT Bearer + Refresh Token (HttpOnly Cookie) |

## Estrutura do Projeto

```
d:/sitema_psicologia/
├── src/
│   ├── PsicoFinance.Api/           # Entry point
│   ├── PsicoFinance.Domain/        # Entidades, Value Objects, Eventos
│   ├── PsicoFinance.Application/   # Use Cases, Commands, Queries
│   ├── PsicoFinance.Infrastructure/# EF Core, Repositórios, Jobs
│   └── PsicoFinance.Tests/         # xUnit + Testcontainers
├── frontend/                        # Angular 18+
└── sql/                             # Scripts SQL (Supabase)
```

## Pré-requisitos

- .NET 8 SDK
- Node.js 20+
- PostgreSQL 16 (ou Supabase)
- N8N (para automações)

## Configuração

1. Copie `src/PsicoFinance.Api/appsettings.Development.json.example` para `appsettings.Development.json`
2. Preencha as variáveis de ambiente (ver `.env.example`)
3. Execute as migrations: `dotnet ef database update`
4. Inicie a API: `dotnet run --project src/PsicoFinance.Api`
5. Inicie o frontend: `cd frontend && npm start`

## Multi-tenancy

Cada clínica possui seu próprio schema no PostgreSQL. O `TenantId` é resolvido via JWT e injetado automaticamente em todas as queries via EF Core `HasQueryFilter`.

## Relatórios Personalizados (B.I.)

O sistema inclui um módulo completo de Business Intelligence com:

- **Motor de relatórios** com filtros dinâmicos, agrupamentos e ordenação configuráveis
- **Templates pré-definidos**: faturamento, produtividade por psicólogo, inadimplência (aging 30/60/90), comparativo mensal, ocupação de agenda, repasses e fluxo de caixa projetado
- **Relatórios personalizados**: o usuário pode criar seus próprios relatórios com construtor visual de filtros
- **Exportação**: PDF, Excel (.xlsx) e CSV
- **Visualização interativa**: gráficos de barras, linhas, pizza e área com drill-down
- **Agendamento automático**: envio periódico por email/WhatsApp via integração N8N

## Segurança (LGPD)

Dados sensíveis de pacientes (CPF, telefone, email) são criptografados em repouso com AES-256.

---

*Versão: 1.0 | Arquitetura: Modular Monolith*
