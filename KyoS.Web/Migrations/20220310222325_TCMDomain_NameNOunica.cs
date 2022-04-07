using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMDomain_NameNOunica : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TCMDomains_Name",
                table: "TCMDomains");

            migrationBuilder.CreateIndex(
                name: "IX_TCMDomains_Id",
                table: "TCMDomains",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TCMDomains_Id",
                table: "TCMDomains");

            migrationBuilder.CreateIndex(
                name: "IX_TCMDomains_Name",
                table: "TCMDomains",
                column: "Name",
                unique: true);
        }
    }
}
