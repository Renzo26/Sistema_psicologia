-- ============================================================
-- 04_financeiro.sql
-- Lançamentos Financeiros, Repasses, Fechamentos Mensais
-- ============================================================


-- ------------------------------------------------------------
-- LANÇAMENTOS FINANCEIROS
-- ------------------------------------------------------------
CREATE TYPE tipo_lancamento   AS ENUM ('receita', 'despesa');
CREATE TYPE status_lancamento AS ENUM ('previsto', 'confirmado', 'cancelado');
CREATE TYPE origem_lancamento AS ENUM ('sessao', 'manual', 'repasse', 'estorno');

CREATE TABLE lancamentos_financeiros (
    id                  UUID                PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id          UUID                NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,

    descricao           VARCHAR(300)        NOT NULL,
    tipo                tipo_lancamento     NOT NULL,
    status              status_lancamento   NOT NULL DEFAULT 'previsto',
    origem              origem_lancamento   NOT NULL DEFAULT 'manual',

    valor               NUMERIC(10,2)       NOT NULL,
    data_vencimento     DATE                NOT NULL,
    data_competencia    DATE                NOT NULL,   -- mês de referência
    data_pagamento      DATE,                           -- preenchido ao confirmar

    -- Relacionamentos opcionais (quando origem = 'sessao')
    sessao_id           UUID                REFERENCES sessoes(id),
    paciente_id         UUID                REFERENCES pacientes(id),
    psicologo_id        UUID                REFERENCES psicologos(id),
    contrato_id         UUID                REFERENCES contratos(id),

    -- Categorização
    plano_conta_id      UUID                REFERENCES planos_conta(id),
    forma_pagamento     forma_pagamento,

    -- Controle de período fechado
    fechamento_id       UUID,               -- FK adicionada após criar fechamentos_mensais
    periodo_fechado     BOOLEAN             NOT NULL DEFAULT FALSE,

    -- Auditoria de alterações (LGPD / compliance financeiro)
    criado_por          UUID                REFERENCES usuarios(id),
    atualizado_por      UUID                REFERENCES usuarios(id),

    observacoes         TEXT,

    criado_em           TIMESTAMPTZ         NOT NULL DEFAULT NOW(),
    atualizado_em       TIMESTAMPTZ         NOT NULL DEFAULT NOW(),
    excluido_em         TIMESTAMPTZ
);

COMMENT ON TABLE lancamentos_financeiros IS 'Lançamentos de receitas e despesas. Gerados automaticamente pelas sessões ou manualmente.';
COMMENT ON COLUMN lancamentos_financeiros.data_competencia IS 'Mês de referência do lançamento (primeiro dia do mês). Ex: 2024-03-01 = março/2024.';
COMMENT ON COLUMN lancamentos_financeiros.periodo_fechado IS 'TRUE = período foi fechado, lançamento não pode ser editado.';

CREATE TRIGGER trg_lancamentos_atualizado_em
    BEFORE UPDATE ON lancamentos_financeiros
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();

-- FK retroativa: sessões referenciam lançamentos
ALTER TABLE sessoes
    ADD CONSTRAINT fk_sessoes_lancamento
    FOREIGN KEY (lancamento_id) REFERENCES lancamentos_financeiros(id) ON DELETE SET NULL;


-- ------------------------------------------------------------
-- HISTÓRICO DE AUDITORIA DOS LANÇAMENTOS
-- (registra cada alteração: valor anterior x novo)
-- ------------------------------------------------------------
CREATE TABLE lancamentos_auditoria (
    id                  UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    lancamento_id       UUID        NOT NULL REFERENCES lancamentos_financeiros(id),
    clinica_id          UUID        NOT NULL REFERENCES clinicas(id),
    usuario_id          UUID        REFERENCES usuarios(id),

    campo_alterado      VARCHAR(50) NOT NULL,
    valor_anterior      TEXT,
    valor_novo          TEXT,

    criado_em           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE lancamentos_auditoria IS 'Trilha de auditoria de alterações em lançamentos financeiros (RNF06).';


-- ------------------------------------------------------------
-- REPASSES PARA PSICÓLOGOS PJ
-- ------------------------------------------------------------
CREATE TYPE status_repasse AS ENUM ('calculado', 'pago', 'cancelado');

CREATE TABLE repasses (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id          UUID            NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    psicologo_id        UUID            NOT NULL REFERENCES psicologos(id),

    -- Período de referência
    mes_referencia      DATE            NOT NULL,  -- primeiro dia do mês. Ex: 2024-03-01

    -- Valores calculados
    total_sessoes       INTEGER         NOT NULL DEFAULT 0,
    valor_bruto         NUMERIC(10,2)   NOT NULL DEFAULT 0,  -- soma dos valores das sessões realizadas
    valor_repasse       NUMERIC(10,2)   NOT NULL DEFAULT 0,  -- valor calculado a pagar
    percentual_aplicado NUMERIC(5,2),                        -- % aplicado (se tipo = percentual)

    status              status_repasse  NOT NULL DEFAULT 'calculado',
    data_pagamento      DATE,

    -- Lançamento financeiro de saída gerado pelo repasse
    lancamento_id       UUID            REFERENCES lancamentos_financeiros(id),

    observacoes         TEXT,
    criado_por          UUID            REFERENCES usuarios(id),

    criado_em           TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    atualizado_em       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),

    UNIQUE (clinica_id, psicologo_id, mes_referencia)
);

COMMENT ON TABLE repasses IS 'Repasses mensais calculados para psicólogos PJ. Um registro por psicólogo por mês.';

CREATE TRIGGER trg_repasses_atualizado_em
    BEFORE UPDATE ON repasses
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();


-- ------------------------------------------------------------
-- FECHAMENTOS MENSAIS
-- ------------------------------------------------------------
CREATE TYPE status_fechamento AS ENUM ('aberto', 'fechado');

CREATE TABLE fechamentos_mensais (
    id                      UUID                PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id              UUID                NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,

    mes_referencia          DATE                NOT NULL,  -- primeiro dia do mês
    status                  status_fechamento   NOT NULL DEFAULT 'aberto',

    -- Totalizadores calculados no fechamento
    total_receita_prevista  NUMERIC(10,2)       NOT NULL DEFAULT 0,
    total_receita_realizada NUMERIC(10,2)       NOT NULL DEFAULT 0,
    total_despesa           NUMERIC(10,2)       NOT NULL DEFAULT 0,
    total_repasses          NUMERIC(10,2)       NOT NULL DEFAULT 0,
    saldo_final             NUMERIC(10,2)       NOT NULL DEFAULT 0,

    total_sessoes_agendadas INTEGER             NOT NULL DEFAULT 0,
    total_sessoes_realizadas INTEGER            NOT NULL DEFAULT 0,
    total_faltas            INTEGER             NOT NULL DEFAULT 0,

    -- Quem fechou e quando
    fechado_em              TIMESTAMPTZ,
    fechado_por             UUID                REFERENCES usuarios(id),

    observacoes             TEXT,

    criado_em               TIMESTAMPTZ         NOT NULL DEFAULT NOW(),
    atualizado_em           TIMESTAMPTZ         NOT NULL DEFAULT NOW(),

    UNIQUE (clinica_id, mes_referencia)
);

COMMENT ON TABLE fechamentos_mensais IS 'Fechamento mensal da clínica. Após fechado, lançamentos do período ficam bloqueados para edição.';

CREATE TRIGGER trg_fechamentos_atualizado_em
    BEFORE UPDATE ON fechamentos_mensais
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();

-- FK retroativa: lançamentos referenciam o fechamento
ALTER TABLE lancamentos_financeiros
    ADD CONSTRAINT fk_lancamentos_fechamento
    FOREIGN KEY (fechamento_id) REFERENCES fechamentos_mensais(id) ON DELETE SET NULL;


-- ------------------------------------------------------------
-- INDEXES — Financeiro
-- ------------------------------------------------------------
CREATE INDEX idx_lancamentos_clinica_id         ON lancamentos_financeiros(clinica_id);
CREATE INDEX idx_lancamentos_sessao_id          ON lancamentos_financeiros(sessao_id);
CREATE INDEX idx_lancamentos_paciente_id        ON lancamentos_financeiros(paciente_id);
CREATE INDEX idx_lancamentos_psicologo_id       ON lancamentos_financeiros(psicologo_id);
CREATE INDEX idx_lancamentos_status             ON lancamentos_financeiros(status);
CREATE INDEX idx_lancamentos_tipo               ON lancamentos_financeiros(tipo);
CREATE INDEX idx_lancamentos_data_vencimento    ON lancamentos_financeiros(data_vencimento);
CREATE INDEX idx_lancamentos_data_competencia   ON lancamentos_financeiros(data_competencia);
CREATE INDEX idx_lancamentos_periodo_fechado    ON lancamentos_financeiros(periodo_fechado);
-- Index composto para fluxo de caixa
CREATE INDEX idx_lancamentos_clinica_competencia ON lancamentos_financeiros(clinica_id, data_competencia);
CREATE INDEX idx_lancamentos_excluido_em        ON lancamentos_financeiros(excluido_em) WHERE excluido_em IS NULL;

CREATE INDEX idx_repasses_clinica_id            ON repasses(clinica_id);
CREATE INDEX idx_repasses_psicologo_id          ON repasses(psicologo_id);
CREATE INDEX idx_repasses_mes_referencia        ON repasses(mes_referencia);

CREATE INDEX idx_fechamentos_clinica_id         ON fechamentos_mensais(clinica_id);
CREATE INDEX idx_fechamentos_mes_referencia     ON fechamentos_mensais(mes_referencia);
CREATE INDEX idx_fechamentos_status             ON fechamentos_mensais(status);

CREATE INDEX idx_auditoria_lancamento_id        ON lancamentos_auditoria(lancamento_id);
CREATE INDEX idx_auditoria_clinica_id           ON lancamentos_auditoria(clinica_id);
