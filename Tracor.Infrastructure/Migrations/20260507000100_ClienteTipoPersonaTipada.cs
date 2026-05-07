using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tradecorp.Infrastructure.Migrations;

public partial class ClienteTipoPersonaTipada : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE "Clientes"
            SET "TipoPersona" = 'Natural'
            WHERE "TipoPersona" IS NULL;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "TipoPersona",
            table: "Clientes",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(50)",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.AddCheckConstraint(
            name: "CK_Clientes_TipoPersona",
            table: "Clientes",
            sql: "\"TipoPersona\" IN ('Natural', 'Juridica')");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_Clientes_TipoPersona",
            table: "Clientes");

        migrationBuilder.AlterColumn<string>(
            name: "TipoPersona",
            table: "Clientes",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(20)",
            oldMaxLength: 20);
    }
}
