using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMNotesActivityTemp_review11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMNoteActivityTemp_TCMServiceActivity_TCMServiceActivityId",
                table: "TCMNoteActivityTemp");

            migrationBuilder.DropIndex(
                name: "IX_TCMNoteActivityTemp_TCMServiceActivityId",
                table: "TCMNoteActivityTemp");

            migrationBuilder.DropColumn(
                name: "TCMServiceActivityId",
                table: "TCMNoteActivityTemp");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "TCMNoteActivityTemp",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "IdTCMServiceActivity",
                table: "TCMNoteActivityTemp",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "TCMNoteActivity",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdTCMServiceActivity",
                table: "TCMNoteActivityTemp");

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "TCMNoteActivityTemp",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TCMServiceActivityId",
                table: "TCMNoteActivityTemp",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceName",
                table: "TCMNoteActivity",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMNoteActivityTemp_TCMServiceActivityId",
                table: "TCMNoteActivityTemp",
                column: "TCMServiceActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNoteActivityTemp_TCMServiceActivity_TCMServiceActivityId",
                table: "TCMNoteActivityTemp",
                column: "TCMServiceActivityId",
                principalTable: "TCMServiceActivity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
