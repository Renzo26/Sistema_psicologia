-- ============================================================
-- 00_extensoes.sql
-- Extensões necessárias — executar primeiro, como superuser
-- ============================================================

-- UUID v4 para chaves primárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Criptografia para dados sensíveis (LGPD)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Busca textual em português
CREATE EXTENSION IF NOT EXISTS "unaccent";
