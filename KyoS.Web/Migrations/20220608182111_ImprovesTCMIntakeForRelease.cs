using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ImprovesTCMIntakeForRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMIntakeConsentForRelease_TCMClient_TcmClient_FK",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropIndex(
                name: "IX_TCMIntakeConsentForRelease_TcmClient_FK",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.AddColumn<int>(
                name: "TcmClientId",
                table: "TCMIntakeConsentForRelease",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeConsentForRelease_TcmClientId",
                table: "TCMIntakeConsentForRelease",
                column: "TcmClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMIntakeConsentForRelease_TCMClient_TcmClientId",
                table: "TCMIntakeConsentForRelease",
                column: "TcmClientId",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMIntakeConsentForRelease_TCMClient_TcmClientId",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropIndex(
                name: "IX_TCMIntakeConsentForRelease_TcmClientId",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "TcmClientId",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeConsentForRelease_TcmClient_FK",
                table: "TCMIntakeConsentForRelease",
                column: "TcmClient_FK",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMIntakeConsentForRelease_TCMClient_TcmClient_FK",
                table: "TCMIntakeConsentForRelease",
                column: "TcmClient_FK",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
