using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class cantCaracterInCodeService1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TCMServices_Name",
                table: "TCMServices");

            migrationBuilder.CreateIndex(
                name: "IX_TCMServices_Code",
                table: "TCMServices",
                column: "Code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TCMServices_Code",
                table: "TCMServices");

            migrationBuilder.CreateIndex(
                name: "IX_TCMServices_Name",
                table: "TCMServices",
                column: "Name",
                unique: true);
        }
    }
}
