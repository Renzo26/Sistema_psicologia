-- ============================================================
-- 01_auth_tenancy.sql
-- Clínicas (tenants), Usuários e Refresh Tokens
-- ============================================================


-- ------------------------------------------------------------
-- CLÍNICAS (tenant raiz — cada clínica é um tenant isolado)
-- ------------------------------------------------------------
CREATE TABLE clinicas (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    nome                VARCHAR(150)    NOT NULL,
    cnpj                VARCHAR(18)     UNIQUE,
    email               VARCHAR(150)    NOT NULL,
    telefone            VARCHAR(20),
    cep                 VARCHAR(9),
    logradouro          VARCHAR(200),
    numero              VARCHAR(20),
    complemento         VARCHAR(100),
    bairro              VARCHAR(100),
    cidade              VARCHAR(100),
    estado              CHAR(2),

    -- Configurações da clínica
    horario_envio_alerta TIME            DEFAULT '08:00:00',   -- hora do resumo diário WhatsApp
    webhook_n8n_url      VARCHAR(500),                          -- URL base do N8N desta clínica
    timezone             VARCHAR(50)     DEFAULT 'America/Sao_Paulo',

    -- Controle
    ativo               BOOLEAN         NOT NULL DEFAULT TRUE,
    criado_em           TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    atualizado_em       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    excluido_em         TIMESTAMPTZ
);

COMMENT ON TABLE clinicas IS 'Tenant raiz — cada clínica de psicologia é um tenant isolado.';


-- ------------------------------------------------------------
-- USUÁRIOS
-- ------------------------------------------------------------
CREATE TYPE role_usuario AS ENUM ('admin', 'gerente', 'secretaria', 'psicologo');

CREATE TABLE usuarios (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id          UUID            NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,

    nome                VARCHAR(150)    NOT NULL,
    email               VARCHAR(150)    NOT NULL,
    senha_hash          VARCHAR(255)    NOT NULL,   -- BCrypt
    role                role_usuario    NOT NULL DEFAULT 'secretaria',

    -- Vinculo opcional com psicólogo (quando role = psicologo)
    psicologo_id        UUID,                        -- FK adicionada após criar tabela psicologos

    -- Controle de acesso
    ativo               BOOLEAN         NOT NULL DEFAULT TRUE,
    ultimo_acesso_em    TIMESTAMPTZ,

    criado_em           TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    atualizado_em       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    excluido_em         TIMESTAMPTZ,

    UNIQUE (clinica_id, email)
);

COMMENT ON TABLE usuarios IS 'Usuários do sistema. Sempre vinculados a uma clínica (tenant).';
COMMENT ON COLUMN usuarios.senha_hash IS 'Hash BCrypt da senha. Nunca armazenar senha em texto plano.';


-- ------------------------------------------------------------
-- REFRESH TOKENS
-- ------------------------------------------------------------
CREATE TABLE refresh_tokens (
    id              UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    usuario_id      UUID        NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    clinica_id      UUID        NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,

    token_hash      VARCHAR(255) NOT NULL UNIQUE,   -- hash SHA-256 do token
    expira_em       TIMESTAMPTZ  NOT NULL,
    revogado        BOOLEAN      NOT NULL DEFAULT FALSE,
    revogado_em     TIMESTAMPTZ,
    ip_origem       VARCHAR(45),                     -- IPv4 ou IPv6
    user_agent      VARCHAR(500),

    criado_em       TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE refresh_tokens IS 'Refresh tokens JWT. Token real trafega em cookie HttpOnly; aqui fica apenas o hash.';


-- ------------------------------------------------------------
-- TRIGGER: atualiza atualizado_em automaticamente
-- (Reutilizado por todas as tabelas)
-- ------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_set_atualizado_em()
RETURNS TRIGGER AS $$
BEGIN
    NEW.atualizado_em = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_clinicas_atualizado_em
    BEFORE UPDATE ON clinicas
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();

CREATE TRIGGER trg_usuarios_atualizado_em
    BEFORE UPDATE ON usuarios
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();


-- ------------------------------------------------------------
-- INDEXES
-- ------------------------------------------------------------
CREATE INDEX idx_usuarios_clinica_id     ON usuarios(clinica_id);
CREATE INDEX idx_usuarios_email          ON usuarios(email);
CREATE INDEX idx_usuarios_excluido_em    ON usuarios(excluido_em) WHERE excluido_em IS NULL;
CREATE INDEX idx_refresh_tokens_usuario  ON refresh_tokens(usuario_id);
CREATE INDEX idx_refresh_tokens_expira   ON refresh_tokens(expira_em);
