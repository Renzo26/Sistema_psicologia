-- ============================================================
-- 03_sessoes.sql
-- Sessões (agendadas, realizadas, faltas, cancelamentos)
-- ============================================================


-- ------------------------------------------------------------
-- SESSÕES
-- ------------------------------------------------------------
CREATE TYPE status_sessao AS ENUM (
    'agendada',
    'realizada',
    'falta',                -- falta não justificada (cobra)
    'falta_justificada',    -- falta justificada (não cobra, conforme contrato)
    'cancelada'             -- cancelamento pela clínica/psicólogo
);

CREATE TABLE sessoes (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id          UUID            NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    contrato_id         UUID            NOT NULL REFERENCES contratos(id),
    psicologo_id        UUID            NOT NULL REFERENCES psicologos(id),
    paciente_id         UUID            NOT NULL REFERENCES pacientes(id),

    -- Agendamento
    data_sessao         DATE            NOT NULL,
    horario_inicio      TIME            NOT NULL,
    horario_fim         TIME,           -- calculado: horario_inicio + duracao_minutos
    duracao_minutos     INTEGER         NOT NULL DEFAULT 50,

    status              status_sessao   NOT NULL DEFAULT 'agendada',

    -- Valores (copiados do contrato no momento de geração)
    valor_sessao        NUMERIC(10,2)   NOT NULL,
    valor_repasse       NUMERIC(10,2),  -- calculado no fechamento

    -- Controle de alteração de status
    -- Regra: status só pode ser alterado dentro de 30 dias (exceto Admin)
    status_alterado_em  TIMESTAMPTZ,
    status_alterado_por UUID            REFERENCES usuarios(id),

    motivo_cancelamento VARCHAR(300),
    observacoes         TEXT,

    -- Referência ao lançamento financeiro gerado automaticamente
    lancamento_id       UUID,           -- FK adicionada após criar tabela lancamentos_financeiros

    -- Controle de recorrência (para rastrear a origem)
    gerado_automaticamente  BOOLEAN     NOT NULL DEFAULT FALSE,
    recorrencia_origem_id   UUID        REFERENCES sessoes(id),  -- primeira sessão da série

    criado_em           TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    atualizado_em       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    excluido_em         TIMESTAMPTZ
);

COMMENT ON TABLE sessoes IS 'Sessões de psicologia agendadas. Geradas automaticamente pela recorrência do contrato.';
COMMENT ON COLUMN sessoes.valor_sessao IS 'Valor copiado do contrato no momento da geração. Não muda se o contrato for atualizado.';
COMMENT ON COLUMN sessoes.lancamento_id IS 'Lançamento financeiro gerado automaticamente quando a sessão é criada.';


CREATE TRIGGER trg_sessoes_atualizado_em
    BEFORE UPDATE ON sessoes
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();


-- ------------------------------------------------------------
-- INDEXES — Sessões
-- ------------------------------------------------------------
CREATE INDEX idx_sessoes_clinica_id         ON sessoes(clinica_id);
CREATE INDEX idx_sessoes_contrato_id        ON sessoes(contrato_id);
CREATE INDEX idx_sessoes_psicologo_id       ON sessoes(psicologo_id);
CREATE INDEX idx_sessoes_paciente_id        ON sessoes(paciente_id);
CREATE INDEX idx_sessoes_data_sessao        ON sessoes(data_sessao);
CREATE INDEX idx_sessoes_status             ON sessoes(status);
CREATE INDEX idx_sessoes_clinica_data       ON sessoes(clinica_id, data_sessao);
CREATE INDEX idx_sessoes_excluido_em        ON sessoes(excluido_em) WHERE excluido_em IS NULL;

-- Index composto para o job de geração de recorrências
CREATE INDEX idx_sessoes_recorrencia        ON sessoes(contrato_id, data_sessao DESC);
