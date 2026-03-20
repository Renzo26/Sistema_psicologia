-- ============================================================
-- 07_seeds.sql
-- Dados iniciais obrigatórios (executar após criar as tabelas)
-- ============================================================


-- ------------------------------------------------------------
-- PLANOS DE CONTA PADRÃO
-- (inseridos na clínica de demonstração — ajustar clinica_id)
-- ------------------------------------------------------------

-- Para usar: substitua '00000000-0000-0000-0000-000000000000'
-- pelo UUID real da clínica após criá-la.

/*
INSERT INTO planos_conta (clinica_id, nome, tipo) VALUES
    -- Receitas
    ('SEU_CLINICA_ID', 'Sessão de Psicologia',          'receita'),
    ('SEU_CLINICA_ID', 'Sessão via Convênio',            'receita'),
    ('SEU_CLINICA_ID', 'Avaliação Psicológica',          'receita'),
    ('SEU_CLINICA_ID', 'Laudo / Relatório',              'receita'),
    ('SEU_CLINICA_ID', 'Outros Serviços',                'receita'),
    -- Despesas
    ('SEU_CLINICA_ID', 'Aluguel',                        'despesa'),
    ('SEU_CLINICA_ID', 'Repasse Psicólogo PJ',           'despesa'),
    ('SEU_CLINICA_ID', 'Material de Escritório',         'despesa'),
    ('SEU_CLINICA_ID', 'Software / Assinaturas',         'despesa'),
    ('SEU_CLINICA_ID', 'Contador / Honorários',          'despesa'),
    ('SEU_CLINICA_ID', 'Marketing',                      'despesa'),
    ('SEU_CLINICA_ID', 'Impostos e Taxas',               'despesa'),
    ('SEU_CLINICA_ID', 'Outros',                         'despesa');
*/


-- ------------------------------------------------------------
-- COMO CRIAR A PRIMEIRA CLÍNICA E USUÁRIO ADMIN
-- (exemplo de uso — ajuste os valores)
-- ------------------------------------------------------------

/*
-- 1. Criar a clínica
INSERT INTO clinicas (nome, cnpj, email, telefone, cidade, estado)
VALUES ('Clínica Exemplo', '00.000.000/0001-00', 'contato@clinica.com.br', '(11) 99999-9999', 'São Paulo', 'SP')
RETURNING id;  -- copie o UUID gerado

-- 2. Criar o usuário admin
-- A senha deve ser gerada como hash BCrypt no backend antes de inserir.
-- Exemplo de hash BCrypt para 'Admin@123': $2a$12$...
INSERT INTO usuarios (clinica_id, nome, email, senha_hash, role)
VALUES (
    'UUID_DA_CLINICA_AQUI',
    'Administrador',
    'admin@clinica.com.br',
    '$2a$12$HASH_BCRYPT_AQUI',
    'admin'
);
*/
