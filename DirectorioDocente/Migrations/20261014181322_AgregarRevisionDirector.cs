using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectorioDocente.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRevisionDirector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComentarioDirector",
                table: "ActividadesDocentes",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRevision",
                table: "ActividadesDocentes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComentarioDirector",
                table: "ActividadesDocentes");

            migrationBuilder.DropColumn(
                name: "FechaRevision",
                table: "ActividadesDocentes");
        }
    }
}
