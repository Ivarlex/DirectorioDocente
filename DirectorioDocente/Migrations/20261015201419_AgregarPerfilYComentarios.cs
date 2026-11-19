using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectorioDocente.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPerfilYComentarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Biografia",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Correo",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreCompleto",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TituloCargo",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Biografia",
                table: "Docentes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cubiculo",
                table: "Docentes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Especialidad",
                table: "Docentes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Docentes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TituloAcademico",
                table: "Docentes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComentariosActividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActividadDocenteId = table.Column<int>(type: "INTEGER", nullable: false),
                    AutorNombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    RolAutor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Mensaje = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComentariosActividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComentariosActividades_ActividadesDocentes_ActividadDocenteId",
                        column: x => x.ActividadDocenteId,
                        principalTable: "ActividadesDocentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Biografia", "Correo", "FotoUrl", "NombreCompleto", "Telefono", "TituloCargo" },
                values: new object[] { null, null, null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosActividades_ActividadDocenteId",
                table: "ComentariosActividades",
                column: "ActividadDocenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComentariosActividades");

            migrationBuilder.DropColumn(
                name: "Biografia",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Correo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "NombreCompleto",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TituloCargo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Biografia",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "Cubiculo",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "Especialidad",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "TituloAcademico",
                table: "Docentes");
        }
    }
}
