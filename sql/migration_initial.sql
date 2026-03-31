CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE audit_logs (
        id uuid NOT NULL,
        clinica_id uuid NOT NULL,
        usuario_id uuid,
        acao character varying(50) NOT NULL,
        entidade character varying(100) NOT NULL,
        entidade_id uuid NOT NULL,
        dados_anteriores jsonb,
        dados_novos jsonb,
        ip_origem character varying(45),
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        CONSTRAINT pk_audit_logs PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE clinicas (
        id uuid NOT NULL,
        nome character varying(150) NOT NULL,
        cnpj character varying(18),
        email character varying(150) NOT NULL,
        telefone character varying(20),
        cep character varying(9),
        logradouro character varying(200),
        numero character varying(20),
        complemento character varying(100),
        bairro character varying(100),
        cidade character varying(100),
        estado character(2),
        horario_envio_alerta time without time zone NOT NULL,
        webhook_n8n_url character varying(500),
        timezone character varying(50) NOT NULL DEFAULT 'America/Sao_Paulo',
        ativo boolean NOT NULL,
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        CONSTRAINT pk_clinicas PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE pacientes (
        id uuid NOT NULL,
        nome character varying(150) NOT NULL,
        cpf text,
        email text,
        telefone text,
        data_nascimento date,
        responsavel_nome character varying(150),
        responsavel_telefone text,
        observacoes text,
        ativo boolean NOT NULL,
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        clinica_id uuid NOT NULL,
        CONSTRAINT pk_pacientes PRIMARY KEY (id),
        CONSTRAINT fk_pacientes_clinicas_clinica_id FOREIGN KEY (clinica_id) REFERENCES clinicas (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE planos_conta (
        id uuid NOT NULL,
        nome character varying(100) NOT NULL,
        tipo character varying(10) NOT NULL,
        descricao character varying(300),
        ativo boolean NOT NULL,
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        clinica_id uuid NOT NULL,
        CONSTRAINT pk_planos_conta PRIMARY KEY (id),
        CONSTRAINT fk_planos_conta_clinicas_clinica_id FOREIGN KEY (clinica_id) REFERENCES clinicas (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE psicologos (
        id uuid NOT NULL,
        nome character varying(150) NOT NULL,
        crp character varying(20) NOT NULL,
        email character varying(150),
        telefone character varying(20),
        cpf character varying(14),
        tipo character varying(10) NOT NULL,
        tipo_repasse character varying(20) NOT NULL,
        valor_repasse numeric(10,2) NOT NULL,
        banco character varying(100),
        agencia character varying(20),
        conta character varying(30),
        pix_chave text,
        ativo boolean NOT NULL,
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        clinica_id uuid NOT NULL,
        CONSTRAINT pk_psicologos PRIMARY KEY (id),
        CONSTRAINT fk_psicologos_clinicas_clinica_id FOREIGN KEY (clinica_id) REFERENCES clinicas (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE contratos (
        id uuid NOT NULL,
        paciente_id uuid NOT NULL,
        psicologo_id uuid NOT NULL,
        valor_sessao numeric(10,2) NOT NULL,
        forma_pagamento character varying(20) NOT NULL,
        frequencia character varying(15) NOT NULL,
        dia_semanasessao character varying(10) NOT NULL,
        horario_sessao time without time zone NOT NULL,
        duracao_minutos integer NOT NULL DEFAULT 50,
        cobra_falta_injustificada boolean NOT NULL,
        cobra_falta_justificada boolean NOT NULL,
        data_inicio date NOT NULL,
        data_fim date,
        status character varying(15) NOT NULL,
        motivo_encerramento character varying(300),
        plano_conta_id uuid,
        observacoes text,
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        clinica_id uuid NOT NULL,
        CONSTRAINT pk_contratos PRIMARY KEY (id),
        CONSTRAINT fk_contratos__pacientes_paciente_id FOREIGN KEY (paciente_id) REFERENCES pacientes (id) ON DELETE RESTRICT,
        CONSTRAINT fk_contratos__planos_conta_plano_conta_id FOREIGN KEY (plano_conta_id) REFERENCES planos_conta (id) ON DELETE SET NULL,
        CONSTRAINT fk_contratos__psicologos_psicologo_id FOREIGN KEY (psicologo_id) REFERENCES psicologos (id) ON DELETE RESTRICT,
        CONSTRAINT fk_contratos_clinicas_clinica_id FOREIGN KEY (clinica_id) REFERENCES clinicas (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE repasses (
        id uuid NOT NULL,
        psicologo_id uuid NOT NULL,
        mes_referencia character varying(7) NOT NULL,
        valor_calculado numeric(18,2) NOT NULL,
        total_sessoes integer NOT NULL,
        status character varying(15) NOT NULL,
        data_pagamento date,
        observacao character varying(500),
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        clinica_id uuid NOT NULL,
        CONSTRAINT pk_repasses PRIMARY KEY (id),
        CONSTRAINT fk_repasses_clinicas_clinica_id FOREIGN KEY (clinica_id) REFERENCES clinicas (id) ON DELETE CASCADE,
        CONSTRAINT fk_repasses_psicologos_psicologo_id FOREIGN KEY (psicologo_id) REFERENCES psicologos (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE usuarios (
        id uuid NOT NULL,
        nome character varying(150) NOT NULL,
        email character varying(150) NOT NULL,
        senha_hash character varying(255) NOT NULL,
        role character varying(20) NOT NULL,
        psicologo_id uuid,
        ativo boolean NOT NULL,
        ultimo_acesso_em timestamptz,
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        clinica_id uuid NOT NULL,
        CONSTRAINT pk_usuarios PRIMARY KEY (id),
        CONSTRAINT fk_usuarios_clinicas_clinica_id FOREIGN KEY (clinica_id) REFERENCES clinicas (id) ON DELETE CASCADE,
        CONSTRAINT fk_usuarios_psicologos_psicologo_id FOREIGN KEY (psicologo_id) REFERENCES psicologos (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE sessoes (
        id uuid NOT NULL,
        contrato_id uuid NOT NULL,
        paciente_id uuid NOT NULL,
        psicologo_id uuid NOT NULL,
        data date NOT NULL,
        horario_inicio time without time zone NOT NULL,
        duracao_minutos integer NOT NULL DEFAULT 50,
        status character varying(20) NOT NULL,
        observacoes character varying(2000),
        motivo_falta character varying(500),
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        clinica_id uuid NOT NULL,
        CONSTRAINT pk_sessoes PRIMARY KEY (id),
        CONSTRAINT fk_sessoes_clinicas_clinica_id FOREIGN KEY (clinica_id) REFERENCES clinicas (id) ON DELETE CASCADE,
        CONSTRAINT fk_sessoes_contratos_contrato_id FOREIGN KEY (contrato_id) REFERENCES contratos (id) ON DELETE RESTRICT,
        CONSTRAINT fk_sessoes_pacientes_paciente_id FOREIGN KEY (paciente_id) REFERENCES pacientes (id) ON DELETE RESTRICT,
        CONSTRAINT fk_sessoes_psicologos_psicologo_id FOREIGN KEY (psicologo_id) REFERENCES psicologos (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE refresh_tokens (
        id uuid NOT NULL,
        usuario_id uuid NOT NULL,
        token_hash character varying(255) NOT NULL,
        expira_em timestamptz NOT NULL,
        revogado boolean NOT NULL,
        revogado_em timestamptz,
        ip_origem character varying(45),
        user_agent character varying(500),
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        clinica_id uuid NOT NULL,
        CONSTRAINT pk_refresh_tokens PRIMARY KEY (id),
        CONSTRAINT fk_refresh_tokens__usuarios_usuario_id FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE CASCADE,
        CONSTRAINT fk_refresh_tokens_clinicas_clinica_id FOREIGN KEY (clinica_id) REFERENCES clinicas (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE TABLE lancamentos_financeiros (
        id uuid NOT NULL,
        descricao character varying(200) NOT NULL,
        valor numeric(18,2) NOT NULL,
        tipo character varying(10) NOT NULL,
        status character varying(15) NOT NULL,
        data_vencimento date NOT NULL,
        data_pagamento date,
        competencia character varying(7) NOT NULL,
        sessao_id uuid,
        plano_conta_id uuid NOT NULL,
        observacao character varying(500),
        criado_em timestamptz NOT NULL,
        atualizado_em timestamptz NOT NULL,
        excluido_em timestamptz,
        clinica_id uuid NOT NULL,
        CONSTRAINT pk_lancamentos_financeiros PRIMARY KEY (id),
        CONSTRAINT fk_lancamentos_financeiros__planos_conta_plano_conta_id FOREIGN KEY (plano_conta_id) REFERENCES planos_conta (id) ON DELETE RESTRICT,
        CONSTRAINT fk_lancamentos_financeiros__sessoes_sessao_id FOREIGN KEY (sessao_id) REFERENCES sessoes (id) ON DELETE SET NULL,
        CONSTRAINT fk_lancamentos_financeiros_clinicas_clinica_id FOREIGN KEY (clinica_id) REFERENCES clinicas (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_audit_logs_clinica_id ON audit_logs (clinica_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_audit_logs_criado_em ON audit_logs (criado_em);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_audit_logs_entidade_entidade_id ON audit_logs (entidade, entidade_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_clinicas_cnpj ON clinicas (cnpj) WHERE cnpj IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_contratos_clinica_id ON contratos (clinica_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_contratos_paciente_id ON contratos (paciente_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_contratos_plano_conta_id ON contratos (plano_conta_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_contratos_psicologo_id ON contratos (psicologo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_contratos_status ON contratos (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_lancamentos_financeiros_clinica_id_competencia_status ON lancamentos_financeiros (clinica_id, competencia, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_lancamentos_financeiros_clinica_id_data_vencimento ON lancamentos_financeiros (clinica_id, data_vencimento);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_lancamentos_financeiros_plano_conta_id ON lancamentos_financeiros (plano_conta_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_lancamentos_financeiros_sessao_id ON lancamentos_financeiros (sessao_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_pacientes_clinica_id ON pacientes (clinica_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_planos_conta_clinica_id_nome_tipo ON planos_conta (clinica_id, nome, tipo);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_psicologos_clinica_id_crp ON psicologos (clinica_id, crp);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_refresh_tokens_clinica_id ON refresh_tokens (clinica_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_refresh_tokens_expira_em ON refresh_tokens (expira_em);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_refresh_tokens_token_hash ON refresh_tokens (token_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_refresh_tokens_usuario_id ON refresh_tokens (usuario_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_repasses_clinica_id_psicologo_id_mes_referencia ON repasses (clinica_id, psicologo_id, mes_referencia);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_repasses_psicologo_id ON repasses (psicologo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_sessoes_clinica_id ON sessoes (clinica_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_sessoes_contrato_id_data ON sessoes (contrato_id, data);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_sessoes_data ON sessoes (data);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_sessoes_paciente_id ON sessoes (paciente_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_sessoes_psicologo_id ON sessoes (psicologo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_sessoes_status ON sessoes (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_usuarios_clinica_id_email ON usuarios (clinica_id, email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_usuarios_email ON usuarios (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    CREATE INDEX ix_usuarios_psicologo_id ON usuarios (psicologo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260331000731_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260331000731_InitialCreate', '8.0.11');
    END IF;
END $EF$;
COMMIT;

