using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectorioDocente.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUsuariosYDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentosActividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActividadDocenteId = table.Column<int>(type: "INTEGER", nullable: false),
                    NombreOriginal = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    NombreAlmacenado = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    RutaRelativa = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TamanoBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubidoPor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosActividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosActividades_ActividadesDocentes_ActividadDocenteId",
                        column: x => x.ActividadDocenteId,
                        principalTable: "ActividadesDocentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreUsuario = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Rol = table.Column<int>(type: "INTEGER", nullable: false),
                    DocenteId = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Docentes_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "Docentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "DocenteId", "FechaCreacion", "NombreUsuario", "PasswordHash", "Rol" },
                values: new object[] { 1, true, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "director", "8f14e45fceea167a5a36dedd4bea2543b9ecca02b49ede9a571527a5e6fb9c9a", 0 });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosActividades_ActividadDocenteId",
                table: "DocumentosActividades",
                column: "ActividadDocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_DocenteId",
                table: "Usuarios",
                column: "DocenteId");

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
                name: "DocumentosActividades");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
