using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMAdendumVersion4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TcmDomainId",
                table: "TCMAdendums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMAdendums_TcmDomainId",
                table: "TCMAdendums",
                column: "TcmDomainId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMAdendums_TCMDomains_TcmDomainId",
                table: "TCMAdendums",
                column: "TcmDomainId",
                principalTable: "TCMDomains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMAdendums_TCMDomains_TcmDomainId",
                table: "TCMAdendums");

            migrationBuilder.DropIndex(
                name: "IX_TCMAdendums_TcmDomainId",
                table: "TCMAdendums");

            migrationBuilder.DropColumn(
                name: "TcmDomainId",
                table: "TCMAdendums");
        }
    }
}
