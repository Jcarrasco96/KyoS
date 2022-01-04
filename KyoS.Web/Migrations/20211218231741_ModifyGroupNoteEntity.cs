using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyGroupNoteEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Activities_GroupNotes_GroupNoteEntityId",
                table: "Notes_Activities");

            migrationBuilder.DropIndex(
                name: "IX_Notes_Activities_GroupNoteEntityId",
                table: "Notes_Activities");

            migrationBuilder.DropColumn(
                name: "GroupNoteEntityId",
                table: "Notes_Activities");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupNoteEntityId",
                table: "Notes_Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Activities_GroupNoteEntityId",
                table: "Notes_Activities",
                column: "GroupNoteEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Activities_GroupNotes_GroupNoteEntityId",
                table: "Notes_Activities",
                column: "GroupNoteEntityId",
                principalTable: "GroupNotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
