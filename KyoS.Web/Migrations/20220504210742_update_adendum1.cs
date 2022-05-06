using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class update_adendum1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adendums_MTPs_MtpId",
                table: "Adendums");

            migrationBuilder.AddForeignKey(
                name: "FK_Adendums_MTPs_MtpId",
                table: "Adendums",
                column: "MtpId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adendums_MTPs_MtpId",
                table: "Adendums");

            migrationBuilder.AddForeignKey(
                name: "FK_Adendums_MTPs_MtpId",
                table: "Adendums",
                column: "MtpId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
