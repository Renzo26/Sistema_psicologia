using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsicoFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRelatorioPersonalizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "relatorios_personalizados",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    filtros_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    agrupamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ordenacao = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    favorito = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clinica_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    excluido_em = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_relatorios_personalizados", x => x.id);
                    table.ForeignKey(
                        name: "fk_relatorios_personalizados_clinicas_clinica_id",
                        column: x => x.clinica_id,
                        principalTable: "clinicas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_relatorios_personalizados_clinica_id",
                table: "relatorios_personalizados",
                column: "clinica_id");

            migrationBuilder.CreateIndex(
                name: "ix_relatorios_personalizados_tipo",
                table: "relatorios_personalizados",
                column: "tipo");

            migrationBuilder.CreateIndex(
                name: "ix_relatorios_personalizados_favorito",
                table: "relatorios_personalizados",
                column: "favorito");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "relatorios_personalizados");
        }
    }
}
