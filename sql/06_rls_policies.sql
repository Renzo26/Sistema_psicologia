-- ============================================================
-- 06_rls_policies.sql
-- Row Level Security (RLS) — Isolamento multi-tenant no Supabase
--
-- COMO FUNCIONA:
-- O backend (.NET) seta a variável de sessão "app.clinica_id"
-- ao iniciar cada request autenticado.
-- As policies usam essa variável para filtrar os dados.
--
-- No .NET, após abrir a conexão:
--   await cmd.ExecuteNonQueryAsync(
--     $"SET app.clinica_id = '{clinicaId}'");
-- ============================================================


-- ------------------------------------------------------------
-- Habilitar RLS em todas as tabelas
-- ------------------------------------------------------------
ALTER TABLE clinicas                ENABLE ROW LEVEL SECURITY;
ALTER TABLE usuarios                ENABLE ROW LEVEL SECURITY;
ALTER TABLE refresh_tokens          ENABLE ROW LEVEL SECURITY;
ALTER TABLE psicologos              ENABLE ROW LEVEL SECURITY;
ALTER TABLE pacientes               ENABLE ROW LEVEL SECURITY;
ALTER TABLE planos_conta            ENABLE ROW LEVEL SECURITY;
ALTER TABLE contratos               ENABLE ROW LEVEL SECURITY;
ALTER TABLE sessoes                 ENABLE ROW LEVEL SECURITY;
ALTER TABLE lancamentos_financeiros ENABLE ROW LEVEL SECURITY;
ALTER TABLE lancamentos_auditoria   ENABLE ROW LEVEL SECURITY;
ALTER TABLE repasses                ENABLE ROW LEVEL SECURITY;
ALTER TABLE fechamentos_mensais     ENABLE ROW LEVEL SECURITY;
ALTER TABLE logs_automacao          ENABLE ROW LEVEL SECURITY;
ALTER TABLE recibos                 ENABLE ROW LEVEL SECURITY;
ALTER TABLE notas_fiscais           ENABLE ROW LEVEL SECURITY;


-- ------------------------------------------------------------
-- FUNÇÃO HELPER: retorna o clinica_id da sessão atual
-- ------------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_clinica_id_atual()
RETURNS UUID AS $$
BEGIN
    RETURN current_setting('app.clinica_id', TRUE)::UUID;
EXCEPTION WHEN OTHERS THEN
    RETURN NULL;
END;
$$ LANGUAGE plpgsql STABLE SECURITY DEFINER;


-- ------------------------------------------------------------
-- POLICIES — clinicas
-- (só vê a própria clínica)
-- ------------------------------------------------------------
CREATE POLICY pol_clinicas_select ON clinicas
    FOR SELECT USING (id = fn_clinica_id_atual());

CREATE POLICY pol_clinicas_update ON clinicas
    FOR UPDATE USING (id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — usuarios
-- ------------------------------------------------------------
CREATE POLICY pol_usuarios_all ON usuarios
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — refresh_tokens
-- ------------------------------------------------------------
CREATE POLICY pol_refresh_tokens_all ON refresh_tokens
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — psicologos
-- ------------------------------------------------------------
CREATE POLICY pol_psicologos_all ON psicologos
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — pacientes
-- ------------------------------------------------------------
CREATE POLICY pol_pacientes_all ON pacientes
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — planos_conta
-- ------------------------------------------------------------
CREATE POLICY pol_planos_conta_all ON planos_conta
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — contratos
-- ------------------------------------------------------------
CREATE POLICY pol_contratos_all ON contratos
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — sessoes
-- ------------------------------------------------------------
CREATE POLICY pol_sessoes_all ON sessoes
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — lancamentos_financeiros
-- ------------------------------------------------------------
CREATE POLICY pol_lancamentos_all ON lancamentos_financeiros
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — lancamentos_auditoria
-- ------------------------------------------------------------
CREATE POLICY pol_auditoria_select ON lancamentos_auditoria
    FOR SELECT USING (clinica_id = fn_clinica_id_atual());

-- Auditoria só pode ser inserida, nunca editada ou deletada
CREATE POLICY pol_auditoria_insert ON lancamentos_auditoria
    FOR INSERT WITH CHECK (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — repasses
-- ------------------------------------------------------------
CREATE POLICY pol_repasses_all ON repasses
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — fechamentos_mensais
-- ------------------------------------------------------------
CREATE POLICY pol_fechamentos_all ON fechamentos_mensais
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — logs_automacao
-- ------------------------------------------------------------
CREATE POLICY pol_logs_automacao_all ON logs_automacao
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — recibos
-- ------------------------------------------------------------
CREATE POLICY pol_recibos_all ON recibos
    FOR ALL USING (clinica_id = fn_clinica_id_atual());


-- ------------------------------------------------------------
-- POLICIES — notas_fiscais
-- ------------------------------------------------------------
CREATE POLICY pol_notas_fiscais_all ON notas_fiscais
    FOR ALL USING (clinica_id = fn_clinica_id_atual());
