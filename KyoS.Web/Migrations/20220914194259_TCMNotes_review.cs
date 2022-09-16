using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMNotes_review : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "TCMNoteActivity",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TCMServiceActivityId",
                table: "TCMNoteActivity",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMNoteActivity_TCMServiceActivityId",
                table: "TCMNoteActivity",
                column: "TCMServiceActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNoteActivity_TCMServiceActivity_TCMServiceActivityId",
                table: "TCMNoteActivity",
                column: "TCMServiceActivityId",
                principalTable: "TCMServiceActivity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMNoteActivity_TCMServiceActivity_TCMServiceActivityId",
                table: "TCMNoteActivity");

            migrationBuilder.DropIndex(
                name: "IX_TCMNoteActivity_TCMServiceActivityId",
                table: "TCMNoteActivity");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "TCMNoteActivity");

            migrationBuilder.DropColumn(
                name: "TCMServiceActivityId",
                table: "TCMNoteActivity");
        }
    }
}
