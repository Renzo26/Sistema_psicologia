-- ============================================================
-- 02_cadastros.sql
-- Psicólogos, Pacientes, Contratos, Planos de Conta
-- ============================================================


-- ------------------------------------------------------------
-- PSICÓLOGOS
-- ------------------------------------------------------------
CREATE TYPE tipo_psicologo     AS ENUM ('clt', 'pj');
CREATE TYPE tipo_repasse       AS ENUM ('percentual', 'valor_fixo');

CREATE TABLE psicologos (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id          UUID            NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,

    nome                VARCHAR(150)    NOT NULL,
    crp                 VARCHAR(20)     NOT NULL,
    email               VARCHAR(150),
    telefone            VARCHAR(20),
    cpf                 VARCHAR(14),    -- armazenado criptografado (LGPD)
    tipo                tipo_psicologo  NOT NULL DEFAULT 'pj',

    -- Configuração de repasse
    tipo_repasse        tipo_repasse    NOT NULL DEFAULT 'percentual',
    valor_repasse       NUMERIC(10,2)   NOT NULL DEFAULT 0,
    -- Se tipo_repasse = 'percentual': valor_repasse = 70.00 (significa 70%)
    -- Se tipo_repasse = 'valor_fixo': valor_repasse = 150.00 (por sessão)

    -- Dados bancários para repasse (criptografados)
    banco               VARCHAR(100),
    agencia             VARCHAR(20),
    conta               VARCHAR(30),
    pix_chave           TEXT,           -- criptografado

    ativo               BOOLEAN         NOT NULL DEFAULT TRUE,
    criado_em           TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    atualizado_em       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    excluido_em         TIMESTAMPTZ,

    UNIQUE (clinica_id, crp)
);

COMMENT ON TABLE psicologos IS 'Psicólogos vinculados à clínica. Podem ser CLT ou PJ.';
COMMENT ON COLUMN psicologos.cpf IS 'CPF criptografado com pgcrypto. Use pgp_sym_encrypt/decrypt.';
COMMENT ON COLUMN psicologos.pix_chave IS 'Chave PIX criptografada.';

CREATE TRIGGER trg_psicologos_atualizado_em
    BEFORE UPDATE ON psicologos
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();

-- FK retroativa: usuários podem ser vinculados a um psicólogo
ALTER TABLE usuarios
    ADD CONSTRAINT fk_usuarios_psicologo
    FOREIGN KEY (psicologo_id) REFERENCES psicologos(id) ON DELETE SET NULL;


-- ------------------------------------------------------------
-- PACIENTES
-- ------------------------------------------------------------
CREATE TABLE pacientes (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id          UUID            NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,

    nome                VARCHAR(150)    NOT NULL,
    -- Campos sensíveis LGPD — criptografados com pgcrypto
    cpf                 TEXT,           -- criptografado
    email               TEXT,           -- criptografado
    telefone            TEXT,           -- criptografado
    data_nascimento     DATE,

    -- Dados de contato do responsável (para menores)
    responsavel_nome    VARCHAR(150),
    responsavel_telefone TEXT,          -- criptografado

    observacoes         TEXT,

    ativo               BOOLEAN         NOT NULL DEFAULT TRUE,
    criado_em           TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    atualizado_em       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    excluido_em         TIMESTAMPTZ
);

COMMENT ON TABLE pacientes IS 'Pacientes da clínica. Dados pessoais criptografados conforme LGPD.';
COMMENT ON COLUMN pacientes.cpf IS 'Criptografado. Use pgp_sym_encrypt(cpf, chave) para gravar.';

CREATE TRIGGER trg_pacientes_atualizado_em
    BEFORE UPDATE ON pacientes
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();


-- ------------------------------------------------------------
-- PLANOS DE CONTA (categorias financeiras)
-- ------------------------------------------------------------
CREATE TYPE tipo_plano_conta AS ENUM ('receita', 'despesa');

CREATE TABLE planos_conta (
    id              UUID                PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id      UUID                NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,

    nome            VARCHAR(100)        NOT NULL,
    tipo            tipo_plano_conta    NOT NULL,
    descricao       VARCHAR(300),
    ativo           BOOLEAN             NOT NULL DEFAULT TRUE,

    criado_em       TIMESTAMPTZ         NOT NULL DEFAULT NOW(),
    atualizado_em   TIMESTAMPTZ         NOT NULL DEFAULT NOW(),
    excluido_em     TIMESTAMPTZ,

    UNIQUE (clinica_id, nome, tipo)
);

COMMENT ON TABLE planos_conta IS 'Categorias de lançamentos financeiros (receitas e despesas).';

CREATE TRIGGER trg_planos_conta_atualizado_em
    BEFORE UPDATE ON planos_conta
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();


-- ------------------------------------------------------------
-- CONTRATOS
-- ------------------------------------------------------------
CREATE TYPE status_contrato     AS ENUM ('ativo', 'pausado', 'encerrado');
CREATE TYPE forma_pagamento     AS ENUM ('pix', 'cartao_credito', 'cartao_debito', 'dinheiro', 'convenio', 'transferencia');
CREATE TYPE frequencia_contrato AS ENUM ('semanal', 'quinzenal');
CREATE TYPE dia_semana          AS ENUM ('segunda', 'terca', 'quarta', 'quinta', 'sexta', 'sabado', 'domingo');

CREATE TABLE contratos (
    id                      UUID                PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id              UUID                NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    paciente_id             UUID                NOT NULL REFERENCES pacientes(id),
    psicologo_id            UUID                NOT NULL REFERENCES psicologos(id),

    -- Configuração da sessão
    valor_sessao            NUMERIC(10,2)       NOT NULL,
    forma_pagamento         forma_pagamento     NOT NULL DEFAULT 'pix',
    frequencia              frequencia_contrato NOT NULL DEFAULT 'semanal',
    dia_semana_sessao       dia_semana          NOT NULL,
    horario_sessao          TIME                NOT NULL,
    duracao_minutos         INTEGER             NOT NULL DEFAULT 50,

    -- Regras de falta
    cobra_falta_injustificada   BOOLEAN         NOT NULL DEFAULT TRUE,
    cobra_falta_justificada     BOOLEAN         NOT NULL DEFAULT FALSE,

    -- Vigência
    data_inicio             DATE                NOT NULL,
    data_fim                DATE,               -- NULL = contrato em aberto

    status                  status_contrato     NOT NULL DEFAULT 'ativo',
    motivo_encerramento     VARCHAR(300),

    -- Plano de conta padrão para sessões deste contrato
    plano_conta_id          UUID                REFERENCES planos_conta(id),

    observacoes             TEXT,

    criado_em               TIMESTAMPTZ         NOT NULL DEFAULT NOW(),
    atualizado_em           TIMESTAMPTZ         NOT NULL DEFAULT NOW(),
    excluido_em             TIMESTAMPTZ
);

COMMENT ON TABLE contratos IS 'Contrato entre paciente e psicólogo. Define recorrência, valor e forma de pagamento das sessões.';

CREATE TRIGGER trg_contratos_atualizado_em
    BEFORE UPDATE ON contratos
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();


-- ------------------------------------------------------------
-- INDEXES — Cadastros
-- ------------------------------------------------------------
CREATE INDEX idx_psicologos_clinica_id      ON psicologos(clinica_id);
CREATE INDEX idx_psicologos_excluido_em     ON psicologos(excluido_em) WHERE excluido_em IS NULL;

CREATE INDEX idx_pacientes_clinica_id       ON pacientes(clinica_id);
CREATE INDEX idx_pacientes_excluido_em      ON pacientes(excluido_em) WHERE excluido_em IS NULL;

CREATE INDEX idx_contratos_clinica_id       ON contratos(clinica_id);
CREATE INDEX idx_contratos_paciente_id      ON contratos(paciente_id);
CREATE INDEX idx_contratos_psicologo_id     ON contratos(psicologo_id);
CREATE INDEX idx_contratos_status           ON contratos(status);
CREATE INDEX idx_contratos_excluido_em      ON contratos(excluido_em) WHERE excluido_em IS NULL;

CREATE INDEX idx_planos_conta_clinica_id    ON planos_conta(clinica_id);
