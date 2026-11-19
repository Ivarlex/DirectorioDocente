using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectorioDocente.Migrations
{
    /// <inheritdoc />
    public partial class AgregarInvitacionesDocentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Invitaciones",
                table: "Invitaciones");

            migrationBuilder.RenameTable(
                name: "Invitaciones",
                newName: "InvitacionesDocentes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InvitacionesDocentes",
                table: "InvitacionesDocentes",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_InvitacionesDocentes",
                table: "InvitacionesDocentes");

            migrationBuilder.RenameTable(
                name: "InvitacionesDocentes",
                newName: "Invitaciones");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invitaciones",
                table: "Invitaciones",
                column: "Id");
        }
    }
}
