using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class delete_retation_between_TCMnoteandCasemanager : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMNote_CaseManagers_CaseManagerId",
                table: "TCMNote");

            migrationBuilder.DropIndex(
                name: "IX_TCMNote_CaseManagerId",
                table: "TCMNote");

            migrationBuilder.DropColumn(
                name: "CaseManagerId",
                table: "TCMNote");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CaseManagerId",
                table: "TCMNote",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMNote_CaseManagerId",
                table: "TCMNote",
                column: "CaseManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNote_CaseManagers_CaseManagerId",
                table: "TCMNote",
                column: "CaseManagerId",
                principalTable: "CaseManagers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
