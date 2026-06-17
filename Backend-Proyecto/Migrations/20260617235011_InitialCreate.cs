using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend_Proyecto.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comunicados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<string>(type: "text", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunicados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proyectos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyectos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecuenciasContador",
                columns: table => new
                {
                    Prefijo = table.Column<string>(type: "text", nullable: false),
                    UltimoValor = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecuenciasContador", x => x.Prefijo);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroSeguimiento = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Cedula = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    Correo = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreUsuario = table.Column<string>(type: "text", nullable: false),
                    ContrasenaHash = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Averias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroSeguimiento = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    Correo = table.Column<string>(type: "text", nullable: true),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Prioridad = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FotoNombre = table.Column<string>(type: "text", nullable: true),
                    FotoBase64 = table.Column<string>(type: "text", nullable: true),
                    FontaneroAsignadoId = table.Column<int>(type: "integer", nullable: true),
                    NotasAtencion = table.Column<string>(type: "text", nullable: true),
                    ObservacionesAdmin = table.Column<string>(type: "text", nullable: true),
                    DescripcionTrabajo = table.Column<string>(type: "text", nullable: true),
                    MaterialesUtilizados = table.Column<string>(type: "text", nullable: true),
                    EvidenciaTrabajoNombre = table.Column<string>(type: "text", nullable: true),
                    EvidenciaTrabajoBase64 = table.Column<string>(type: "text", nullable: true),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Averias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Averias_Usuarios_FontaneroAsignadoId",
                        column: x => x.FontaneroAsignadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LecturasMedidor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreAbonado = table.Column<string>(type: "text", nullable: false),
                    NumeroMedidor = table.Column<string>(type: "text", nullable: false),
                    CedulaAbonado = table.Column<string>(type: "text", nullable: true),
                    LecturaAnterior = table.Column<decimal>(type: "numeric", nullable: false),
                    LecturaActual = table.Column<decimal>(type: "numeric", nullable: false),
                    Consumo = table.Column<decimal>(type: "numeric", nullable: false),
                    ConsumoMesAnterior = table.Column<decimal>(type: "numeric", nullable: true),
                    FechaLectura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    FontaneroId = table.Column<int>(type: "integer", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturasMedidor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LecturasMedidor_Usuarios_FontaneroId",
                        column: x => x.FontaneroId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AveriasHistorial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AveriaId = table.Column<int>(type: "integer", nullable: false),
                    Accion = table.Column<string>(type: "text", nullable: false),
                    ValorAnterior = table.Column<string>(type: "text", nullable: true),
                    ValorNuevo = table.Column<string>(type: "text", nullable: true),
                    Usuario = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AveriasHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AveriasHistorial_Averias_AveriaId",
                        column: x => x.AveriaId,
                        principalTable: "Averias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActividadesFontanero",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FontaneroId = table.Column<int>(type: "integer", nullable: false),
                    FechaActividad = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HoraInicio = table.Column<string>(type: "text", nullable: true),
                    HoraFin = table.Column<string>(type: "text", nullable: true),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Ubicacion = table.Column<string>(type: "text", nullable: false),
                    NumeroAveriaVinculada = table.Column<string>(type: "text", nullable: true),
                    LecturaMedidorId = table.Column<int>(type: "integer", nullable: true),
                    MaterialesUtilizados = table.Column<string>(type: "text", nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    EstadoValidacion = table.Column<string>(type: "text", nullable: false),
                    ObservacionValidacion = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AbonadoNumero = table.Column<string>(type: "text", nullable: true),
                    NombreAbonado = table.Column<string>(type: "text", nullable: true),
                    LugarVisita = table.Column<string>(type: "text", nullable: true),
                    MotivoVisita = table.Column<string>(type: "text", nullable: true),
                    LecturaAnteriorM3 = table.Column<decimal>(type: "numeric", nullable: true),
                    LecturaActualM3 = table.Column<decimal>(type: "numeric", nullable: true),
                    ConsumoRegistradoM3 = table.Column<decimal>(type: "numeric", nullable: true),
                    EstadoMedidor = table.Column<string>(type: "text", nullable: true),
                    DetectoFuga = table.Column<string>(type: "text", nullable: true),
                    ResultadoInspeccion = table.Column<string>(type: "text", nullable: true),
                    AccionRecomendada = table.Column<string>(type: "text", nullable: true),
                    FotoMedidorNombre = table.Column<string>(type: "text", nullable: true),
                    FotoMedidorBase64 = table.Column<string>(type: "text", nullable: true),
                    AforoNumero = table.Column<string>(type: "text", nullable: true),
                    LugarPrueba = table.Column<string>(type: "text", nullable: true),
                    HoraPrueba = table.Column<string>(type: "text", nullable: true),
                    ResultadoPsi = table.Column<decimal>(type: "numeric", nullable: true),
                    DiametroTuberia = table.Column<string>(type: "text", nullable: true),
                    ObservacionesPresion = table.Column<string>(type: "text", nullable: true),
                    PruebaNumero = table.Column<string>(type: "text", nullable: true),
                    LugarCasa = table.Column<string>(type: "text", nullable: true),
                    HoraControl = table.Column<string>(type: "text", nullable: true),
                    CloroResidual = table.Column<string>(type: "text", nullable: true),
                    Turbiedad = table.Column<string>(type: "text", nullable: true),
                    Ph = table.Column<decimal>(type: "numeric", nullable: true),
                    Olor = table.Column<string>(type: "text", nullable: true),
                    Sabor = table.Column<string>(type: "text", nullable: true),
                    ObservacionesControlOperativo = table.Column<string>(type: "text", nullable: true),
                    DetalleTrabajoRealizado = table.Column<string>(type: "text", nullable: true),
                    ResultadoTrabajo = table.Column<string>(type: "text", nullable: true),
                    RequiereSeguimiento = table.Column<string>(type: "text", nullable: true),
                    PrioridadSeguimiento = table.Column<string>(type: "text", nullable: true),
                    FotoEvidenciaNombre = table.Column<string>(type: "text", nullable: true),
                    FotoEvidenciaBase64 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActividadesFontanero", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActividadesFontanero_LecturasMedidor_LecturaMedidorId",
                        column: x => x.LecturaMedidorId,
                        principalTable: "LecturasMedidor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ActividadesFontanero_Usuarios_FontaneroId",
                        column: x => x.FontaneroId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesFontanero_FontaneroId",
                table: "ActividadesFontanero",
                column: "FontaneroId");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesFontanero_LecturaMedidorId",
                table: "ActividadesFontanero",
                column: "LecturaMedidorId");

            migrationBuilder.CreateIndex(
                name: "IX_Averias_FontaneroAsignadoId",
                table: "Averias",
                column: "FontaneroAsignadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Averias_NumeroSeguimiento",
                table: "Averias",
                column: "NumeroSeguimiento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AveriasHistorial_AveriaId",
                table: "AveriasHistorial",
                column: "AveriaId");

            migrationBuilder.CreateIndex(
                name: "IX_LecturasMedidor_FontaneroId",
                table: "LecturasMedidor",
                column: "FontaneroId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_NumeroSeguimiento",
                table: "Solicitudes",
                column: "NumeroSeguimiento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActividadesFontanero");

            migrationBuilder.DropTable(
                name: "AveriasHistorial");

            migrationBuilder.DropTable(
                name: "Comunicados");

            migrationBuilder.DropTable(
                name: "Proyectos");

            migrationBuilder.DropTable(
                name: "SecuenciasContador");

            migrationBuilder.DropTable(
                name: "Solicitudes");

            migrationBuilder.DropTable(
                name: "LecturasMedidor");

            migrationBuilder.DropTable(
                name: "Averias");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
