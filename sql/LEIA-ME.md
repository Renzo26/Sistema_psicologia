# SQL — Instruções de Execução no Supabase

Execute os arquivos **na ordem numérica** pelo SQL Editor do Supabase.

## Ordem de execução

| Ordem | Arquivo | Descrição |
|---|---|---|
| 1 | `00_extensoes.sql` | Extensões PostgreSQL (uuid-ossp, pgcrypto, unaccent) |
| 2 | `01_auth_tenancy.sql` | Clínicas (tenants), Usuários, Refresh Tokens |
| 3 | `02_cadastros.sql` | Psicólogos, Pacientes, Contratos, Planos de Conta |
| 4 | `03_sessoes.sql` | Sessões |
| 5 | `04_financeiro.sql` | Lançamentos, Repasses, Fechamentos Mensais |
| 6 | `05_automacoes_documentos.sql` | Logs N8N, Recibos, NFSe |
| 7 | `06_rls_policies.sql` | Row Level Security (isolamento multi-tenant) |
| 8 | `07_seeds.sql` | Dados iniciais (ler os comentários antes de executar) |

## Observações importantes

### Criptografia LGPD
Os campos sensíveis dos pacientes (CPF, email, telefone) e psicólogos (CPF, chave PIX)
são armazenados criptografados. No backend .NET, use:
```
-- Gravar:
pgp_sym_encrypt('valor', 'CHAVE_SECRETA')

-- Ler:
pgp_sym_decrypt(campo::bytea, 'CHAVE_SECRETA')
```
A `CHAVE_SECRETA` deve estar em variável de ambiente, nunca no código.

### Row Level Security (RLS)
O isolamento entre clínicas é feito via RLS. O backend deve setar o `clinica_id`
no início de cada request:
```sql
SET app.clinica_id = 'UUID_DA_CLINICA';
```

### Multi-tenancy
Todas as tabelas possuem `clinica_id`. Nunca faça queries sem filtrar por `clinica_id`.

### Soft Delete
Registros não são deletados fisicamente. O campo `excluido_em` recebe um timestamp.
Sempre filtre `WHERE excluido_em IS NULL` nas queries de listagem.
