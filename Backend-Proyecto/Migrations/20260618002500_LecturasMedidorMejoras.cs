using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend_Proyecto.Migrations;

/// <inheritdoc />
public partial class LecturasMedidorMejoras : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "ConsumoAlto",
            table: "LecturasMedidor",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "EstadoMedidor",
            table: "LecturasMedidor",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "EvidenciaBase64",
            table: "LecturasMedidor",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "EvidenciaNombre",
            table: "LecturasMedidor",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "FechaActualizacion",
            table: "LecturasMedidor",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "HoraLectura",
            table: "LecturasMedidor",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "MotivoVisita",
            table: "LecturasMedidor",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "NumeroAbonado",
            table: "LecturasMedidor",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ObservacionAdmin",
            table: "LecturasMedidor",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ResultadoInspeccion",
            table: "LecturasMedidor",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "RevisadaPorAdminId",
            table: "LecturasMedidor",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Ubicacion",
            table: "LecturasMedidor",
            type: "text",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "HistorialLecturas",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                LecturaMedidorId = table.Column<int>(type: "integer", nullable: false),
                UsuarioId = table.Column<int>(type: "integer", nullable: true),
                UsuarioNombre = table.Column<string>(type: "text", nullable: true),
                Accion = table.Column<string>(type: "text", nullable: false),
                EstadoAnterior = table.Column<string>(type: "text", nullable: true),
                EstadoNuevo = table.Column<string>(type: "text", nullable: true),
                Observacion = table.Column<string>(type: "text", nullable: true),
                Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HistorialLecturas", x => x.Id);
                table.ForeignKey(
                    name: "FK_HistorialLecturas_LecturasMedidor_LecturaMedidorId",
                    column: x => x.LecturaMedidorId,
                    principalTable: "LecturasMedidor",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LecturasMedidor_RevisadaPorAdminId",
            table: "LecturasMedidor",
            column: "RevisadaPorAdminId");

        migrationBuilder.CreateIndex(
            name: "IX_HistorialLecturas_LecturaMedidorId",
            table: "HistorialLecturas",
            column: "LecturaMedidorId");

        migrationBuilder.AddForeignKey(
            name: "FK_LecturasMedidor_Usuarios_RevisadaPorAdminId",
            table: "LecturasMedidor",
            column: "RevisadaPorAdminId",
            principalTable: "Usuarios",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_LecturasMedidor_Usuarios_RevisadaPorAdminId",
            table: "LecturasMedidor");

        migrationBuilder.DropTable(
            name: "HistorialLecturas");

        migrationBuilder.DropIndex(
            name: "IX_LecturasMedidor_RevisadaPorAdminId",
            table: "LecturasMedidor");

        migrationBuilder.DropColumn(name: "ConsumoAlto", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "EstadoMedidor", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "EvidenciaBase64", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "EvidenciaNombre", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "FechaActualizacion", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "HoraLectura", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "MotivoVisita", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "NumeroAbonado", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "ObservacionAdmin", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "ResultadoInspeccion", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "RevisadaPorAdminId", table: "LecturasMedidor");
        migrationBuilder.DropColumn(name: "Ubicacion", table: "LecturasMedidor");
    }
}
