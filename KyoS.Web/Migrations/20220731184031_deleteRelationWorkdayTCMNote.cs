using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class deleteRelationWorkdayTCMNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMNote_Workdays_WorkdayId",
                table: "TCMNote");

            migrationBuilder.DropIndex(
                name: "IX_TCMNote_WorkdayId",
                table: "TCMNote");

            migrationBuilder.DropColumn(
                name: "WorkdayId",
                table: "TCMNote");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkdayId",
                table: "TCMNote",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMNote_WorkdayId",
                table: "TCMNote",
                column: "WorkdayId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMNote_Workdays_WorkdayId",
                table: "TCMNote",
                column: "WorkdayId",
                principalTable: "Workdays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
