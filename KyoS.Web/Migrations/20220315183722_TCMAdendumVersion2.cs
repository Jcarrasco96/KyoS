using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMAdendumVersion2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMAdendumEntity_TCMServicePlans_TcmServicePlanId",
                table: "TCMAdendumEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMAdendumEntity",
                table: "TCMAdendumEntity");

            migrationBuilder.RenameTable(
                name: "TCMAdendumEntity",
                newName: "TCMAdendums");

            migrationBuilder.RenameIndex(
                name: "IX_TCMAdendumEntity_TcmServicePlanId",
                table: "TCMAdendums",
                newName: "IX_TCMAdendums_TcmServicePlanId");

            migrationBuilder.AddColumn<string>(
                name: "Longterm",
                table: "TCMAdendums",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMAdendums",
                table: "TCMAdendums",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TCMAdendums_Id",
                table: "TCMAdendums",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMAdendums_TCMServicePlans_TcmServicePlanId",
                table: "TCMAdendums",
                column: "TcmServicePlanId",
                principalTable: "TCMServicePlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMAdendums_TCMServicePlans_TcmServicePlanId",
                table: "TCMAdendums");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TCMAdendums",
                table: "TCMAdendums");

            migrationBuilder.DropIndex(
                name: "IX_TCMAdendums_Id",
                table: "TCMAdendums");

            migrationBuilder.DropColumn(
                name: "Longterm",
                table: "TCMAdendums");

            migrationBuilder.RenameTable(
                name: "TCMAdendums",
                newName: "TCMAdendumEntity");

            migrationBuilder.RenameIndex(
                name: "IX_TCMAdendums_TcmServicePlanId",
                table: "TCMAdendumEntity",
                newName: "IX_TCMAdendumEntity_TcmServicePlanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TCMAdendumEntity",
                table: "TCMAdendumEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMAdendumEntity_TCMServicePlans_TcmServicePlanId",
                table: "TCMAdendumEntity",
                column: "TcmServicePlanId",
                principalTable: "TCMServicePlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
