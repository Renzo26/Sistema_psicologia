using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsicoFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    acao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dados_anteriores = table.Column<string>(type: "jsonb", nullable: true),
                    dados_novos = table.Column<string>(type: "jsonb", nullable: true),
                    ip_origem = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clinicas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cnpj = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cep = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    complemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estado = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: true),
                    horario_envio_alerta = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    webhook_n8n_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "America/Sao_Paulo"),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clinicas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pacientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cpf = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    responsavel_nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    responsavel_telefone = table.Column<string>(type: "text", nullable: true),
                    observacoes = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pacientes", x => x.id);
                    table.ForeignKey(
                        name: "fk_pacientes_clinicas_clinica_id",
                        column: x => x.clinica_id,
                        principalTable: "clinicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "planos_conta",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_planos_conta", x => x.id);
                    table.ForeignKey(
                        name: "fk_planos_conta_clinicas_clinica_id",
                        column: x => x.clinica_id,
                        principalTable: "clinicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "psicologos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    crp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cpf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    tipo_repasse = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor_repasse = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    banco = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    agencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    conta = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    pix_chave = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_psicologos", x => x.id);
                    table.ForeignKey(
                        name: "fk_psicologos_clinicas_clinica_id",
                        column: x => x.clinica_id,
                        principalTable: "clinicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contratos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    psicologo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_sessao = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    forma_pagamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    frequencia = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    dia_semanasessao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    horario_sessao = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    duracao_minutos = table.Column<int>(type: "integer", nullable: false, defaultValue: 50),
                    cobra_falta_injustificada = table.Column<bool>(type: "boolean", nullable: false),
                    cobra_falta_justificada = table.Column<bool>(type: "boolean", nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    motivo_encerramento = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    plano_conta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observacoes = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contratos", x => x.id);
                    table.ForeignKey(
                        name: "fk_contratos__pacientes_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "pacientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contratos__planos_conta_plano_conta_id",
                        column: x => x.plano_conta_id,
                        principalTable: "planos_conta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_contratos__psicologos_psicologo_id",
                        column: x => x.psicologo_id,
                        principalTable: "psicologos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_contratos_clinicas_clinica_id",
                        column: x => x.clinica_id,
                        principalTable: "clinicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repasses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    psicologo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mes_referencia = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    valor_calculado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_sessoes = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_repasses", x => x.id);
                    table.ForeignKey(
                        name: "fk_repasses_clinicas_clinica_id",
                        column: x => x.clinica_id,
                        principalTable: "clinicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_repasses_psicologos_psicologo_id",
                        column: x => x.psicologo_id,
                        principalTable: "psicologos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    psicologo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_acesso_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuarios_clinicas_clinica_id",
                        column: x => x.clinica_id,
                        principalTable: "clinicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuarios_psicologos_psicologo_id",
                        column: x => x.psicologo_id,
                        principalTable: "psicologos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sessoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paciente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    psicologo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data = table.Column<DateOnly>(type: "date", nullable: false),
                    horario_inicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    duracao_minutos = table.Column<int>(type: "integer", nullable: false, defaultValue: 50),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    motivo_falta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_sessoes_clinicas_clinica_id",
                        column: x => x.clinica_id,
                        principalTable: "clinicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sessoes_contratos_contrato_id",
                        column: x => x.contrato_id,
                        principalTable: "contratos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sessoes_pacientes_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "pacientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sessoes_psicologos_psicologo_id",
                        column: x => x.psicologo_id,
                        principalTable: "psicologos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    revogado = table.Column<bool>(type: "boolean", nullable: false),
                    revogado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    ip_origem = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens__usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_clinicas_clinica_id",
                        column: x => x.clinica_id,
                        principalTable: "clinicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lancamentos_financeiros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    data_vencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    competencia = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    sessao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    plano_conta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lancamentos_financeiros", x => x.id);
                    table.ForeignKey(
                        name: "fk_lancamentos_financeiros__planos_conta_plano_conta_id",
                        column: x => x.plano_conta_id,
                        principalTable: "planos_conta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lancamentos_financeiros__sessoes_sessao_id",
                        column: x => x.sessao_id,
                        principalTable: "sessoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_lancamentos_financeiros_clinicas_clinica_id",
                        column: x => x.clinica_id,
                        principalTable: "clinicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_clinica_id",
                table: "audit_logs",
                column: "clinica_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_criado_em",
                table: "audit_logs",
                column: "criado_em");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entidade_entidade_id",
                table: "audit_logs",
                columns: new[] { "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix_clinicas_cnpj",
                table: "clinicas",
                column: "cnpj",
                unique: true,
                filter: "cnpj IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_contratos_clinica_id",
                table: "contratos",
                column: "clinica_id");

            migrationBuilder.CreateIndex(
                name: "ix_contratos_paciente_id",
                table: "contratos",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_contratos_plano_conta_id",
                table: "contratos",
                column: "plano_conta_id");

            migrationBuilder.CreateIndex(
                name: "ix_contratos_psicologo_id",
                table: "contratos",
                column: "psicologo_id");

            migrationBuilder.CreateIndex(
                name: "ix_contratos_status",
                table: "contratos",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_lancamentos_financeiros_clinica_id_competencia_status",
                table: "lancamentos_financeiros",
                columns: new[] { "clinica_id", "competencia", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_lancamentos_financeiros_clinica_id_data_vencimento",
                table: "lancamentos_financeiros",
                columns: new[] { "clinica_id", "data_vencimento" });

            migrationBuilder.CreateIndex(
                name: "ix_lancamentos_financeiros_plano_conta_id",
                table: "lancamentos_financeiros",
                column: "plano_conta_id");

            migrationBuilder.CreateIndex(
                name: "ix_lancamentos_financeiros_sessao_id",
                table: "lancamentos_financeiros",
                column: "sessao_id");

            migrationBuilder.CreateIndex(
                name: "ix_pacientes_clinica_id",
                table: "pacientes",
                column: "clinica_id");

            migrationBuilder.CreateIndex(
                name: "ix_planos_conta_clinica_id_nome_tipo",
                table: "planos_conta",
                columns: new[] { "clinica_id", "nome", "tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_psicologos_clinica_id_crp",
                table: "psicologos",
                columns: new[] { "clinica_id", "crp" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_clinica_id",
                table: "refresh_tokens",
                column: "clinica_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_expira_em",
                table: "refresh_tokens",
                column: "expira_em");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_usuario_id",
                table: "refresh_tokens",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_repasses_clinica_id_psicologo_id_mes_referencia",
                table: "repasses",
                columns: new[] { "clinica_id", "psicologo_id", "mes_referencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_repasses_psicologo_id",
                table: "repasses",
                column: "psicologo_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_clinica_id",
                table: "sessoes",
                column: "clinica_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_contrato_id_data",
                table: "sessoes",
                columns: new[] { "contrato_id", "data" });

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_data",
                table: "sessoes",
                column: "data");

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_paciente_id",
                table: "sessoes",
                column: "paciente_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_psicologo_id",
                table: "sessoes",
                column: "psicologo_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_status",
                table: "sessoes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_clinica_id_email",
                table: "usuarios",
                columns: new[] { "clinica_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_psicologo_id",
                table: "usuarios",
                column: "psicologo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "lancamentos_financeiros");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "repasses");

            migrationBuilder.DropTable(
                name: "sessoes");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "contratos");

            migrationBuilder.DropTable(
                name: "pacientes");

            migrationBuilder.DropTable(
                name: "planos_conta");

            migrationBuilder.DropTable(
                name: "psicologos");

            migrationBuilder.DropTable(
                name: "clinicas");
        }
    }
}
