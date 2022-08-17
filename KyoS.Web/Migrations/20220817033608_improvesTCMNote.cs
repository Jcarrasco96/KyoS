using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class improvesTCMNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TCMNoteEntityId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_TCMNoteEntityId",
                table: "Messages",
                column: "TCMNoteEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_TCMNote_TCMNoteEntityId",
                table: "Messages",
                column: "TCMNoteEntityId",
                principalTable: "TCMNote",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_TCMNote_TCMNoteEntityId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_TCMNoteEntityId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "TCMNoteEntityId",
                table: "Messages");
        }
    }
}
