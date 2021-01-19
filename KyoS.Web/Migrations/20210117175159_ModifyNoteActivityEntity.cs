using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyNoteActivityEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ObjetiveId",
                table: "Notes_Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Activities_ObjetiveId",
                table: "Notes_Activities",
                column: "ObjetiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Activities_Objetives_ObjetiveId",
                table: "Notes_Activities",
                column: "ObjetiveId",
                principalTable: "Objetives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Activities_Objetives_ObjetiveId",
                table: "Notes_Activities");

            migrationBuilder.DropIndex(
                name: "IX_Notes_Activities_ObjetiveId",
                table: "Notes_Activities");

            migrationBuilder.DropColumn(
                name: "ObjetiveId",
                table: "Notes_Activities");
        }
    }
}
