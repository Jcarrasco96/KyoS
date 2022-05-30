using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMservicesUpdate4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMStages_TCMServices_tCMserviceId",
                table: "TCMStages");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMStages_TCMServices_tCMserviceId",
                table: "TCMStages",
                column: "tCMserviceId",
                principalTable: "TCMServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMStages_TCMServices_tCMserviceId",
                table: "TCMStages");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMStages_TCMServices_tCMserviceId",
                table: "TCMStages",
                column: "tCMserviceId",
                principalTable: "TCMServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
