-- ============================================================
-- 05_automacoes_documentos.sql
-- Logs de Automação N8N, Recibos e Notas Fiscais
-- ============================================================


-- ------------------------------------------------------------
-- LOGS DE AUTOMAÇÃO (N8N webhooks)
-- ------------------------------------------------------------
CREATE TYPE status_automacao AS ENUM ('pendente', 'enviado', 'falha', 'retentar');
CREATE TYPE tipo_evento_automacao AS ENUM (
    'cobranca_vencida',
    'alerta_diario_gestor',
    'repasse_calculado',
    'relatorio_mensal',
    'sessao_confirmada',
    'sessao_cancelada',
    'contrato_encerrado'
);

CREATE TABLE logs_automacao (
    id              UUID                    PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id      UUID                    NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,

    tipo_evento     tipo_evento_automacao   NOT NULL,
    status          status_automacao        NOT NULL DEFAULT 'pendente',

    -- Payload enviado ao N8N
    payload         JSONB                   NOT NULL DEFAULT '{}',
    -- Resposta recebida do N8N
    resposta        JSONB,

    -- Referências opcionais
    sessao_id       UUID                    REFERENCES sessoes(id),
    paciente_id     UUID                    REFERENCES pacientes(id),
    lancamento_id   UUID                    REFERENCES lancamentos_financeiros(id),
    repasse_id      UUID                    REFERENCES repasses(id),

    -- Controle de retry
    tentativas      INTEGER                 NOT NULL DEFAULT 0,
    max_tentativas  INTEGER                 NOT NULL DEFAULT 3,
    proxima_tentativa_em TIMESTAMPTZ,

    -- URL do webhook chamado
    webhook_url     VARCHAR(500),
    http_status_code INTEGER,
    erro_mensagem   TEXT,

    criado_em       TIMESTAMPTZ             NOT NULL DEFAULT NOW(),
    atualizado_em   TIMESTAMPTZ             NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE logs_automacao IS 'Registro de todos os webhooks disparados ao N8N. Suporta retry automático.';

CREATE TRIGGER trg_logs_automacao_atualizado_em
    BEFORE UPDATE ON logs_automacao
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();


-- ------------------------------------------------------------
-- RECIBOS (PDF gerados)
-- ------------------------------------------------------------
CREATE TYPE status_recibo AS ENUM ('gerado', 'enviado', 'cancelado');

CREATE TABLE recibos (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id      UUID            NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    sessao_id       UUID            NOT NULL REFERENCES sessoes(id),
    paciente_id     UUID            NOT NULL REFERENCES pacientes(id),
    lancamento_id   UUID            REFERENCES lancamentos_financeiros(id),

    numero_recibo   VARCHAR(50)     NOT NULL,   -- número sequencial por clínica
    valor           NUMERIC(10,2)   NOT NULL,
    data_emissao    DATE            NOT NULL DEFAULT CURRENT_DATE,

    status          status_recibo   NOT NULL DEFAULT 'gerado',

    -- Armazenamento do arquivo
    arquivo_url     VARCHAR(1000),              -- URL do PDF no storage (Supabase Storage)
    arquivo_nome    VARCHAR(200),

    criado_por      UUID            REFERENCES usuarios(id),
    criado_em       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    atualizado_em   TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE recibos IS 'Recibos PDF gerados para sessões realizadas.';

CREATE TRIGGER trg_recibos_atualizado_em
    BEFORE UPDATE ON recibos
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();


-- ------------------------------------------------------------
-- NOTAS FISCAIS (NFSe)
-- ------------------------------------------------------------
CREATE TYPE status_nfse AS ENUM ('pendente', 'emitida', 'cancelada', 'erro');

CREATE TABLE notas_fiscais (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    clinica_id          UUID            NOT NULL REFERENCES clinicas(id) ON DELETE CASCADE,
    paciente_id         UUID            NOT NULL REFERENCES pacientes(id),
    lancamento_id       UUID            REFERENCES lancamentos_financeiros(id),

    -- Dados da nota
    numero_nfse         VARCHAR(50),            -- número retornado pela prefeitura
    codigo_verificacao  VARCHAR(100),
    data_emissao        TIMESTAMPTZ,
    competencia         DATE            NOT NULL,

    valor_servico       NUMERIC(10,2)   NOT NULL,
    descricao_servico   TEXT            NOT NULL DEFAULT 'Serviços de Psicologia',

    status              status_nfse     NOT NULL DEFAULT 'pendente',
    erro_mensagem       TEXT,

    -- Arquivos
    xml_url             VARCHAR(1000),
    pdf_url             VARCHAR(1000),

    -- Resposta da API da prefeitura
    resposta_api        JSONB,

    criado_por          UUID            REFERENCES usuarios(id),
    criado_em           TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    atualizado_em       TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE notas_fiscais IS 'Notas Fiscais de Serviço Eletrônicas (NFSe) emitidas para pacientes.';

CREATE TRIGGER trg_notas_fiscais_atualizado_em
    BEFORE UPDATE ON notas_fiscais
    FOR EACH ROW EXECUTE FUNCTION fn_set_atualizado_em();


-- ------------------------------------------------------------
-- INDEXES — Automações e Documentos
-- ------------------------------------------------------------
CREATE INDEX idx_logs_automacao_clinica_id      ON logs_automacao(clinica_id);
CREATE INDEX idx_logs_automacao_tipo_evento     ON logs_automacao(tipo_evento);
CREATE INDEX idx_logs_automacao_status          ON logs_automacao(status);
CREATE INDEX idx_logs_automacao_proxima_tent    ON logs_automacao(proxima_tentativa_em) WHERE status = 'retentar';

CREATE INDEX idx_recibos_clinica_id             ON recibos(clinica_id);
CREATE INDEX idx_recibos_sessao_id              ON recibos(sessao_id);
CREATE INDEX idx_recibos_paciente_id            ON recibos(paciente_id);

CREATE INDEX idx_notas_fiscais_clinica_id       ON notas_fiscais(clinica_id);
CREATE INDEX idx_notas_fiscais_paciente_id      ON notas_fiscais(paciente_id);
CREATE INDEX idx_notas_fiscais_status           ON notas_fiscais(status);
CREATE INDEX idx_notas_fiscais_competencia      ON notas_fiscais(competencia);
