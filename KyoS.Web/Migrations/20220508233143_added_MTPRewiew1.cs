using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class added_MTPRewiew1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MTPReviews_MTPs_Client_FK",
                table: "MTPReviews");

            migrationBuilder.RenameColumn(
                name: "Client_FK",
                table: "MTPReviews",
                newName: "MTP_FK");

            migrationBuilder.RenameIndex(
                name: "IX_MTPReviews_Client_FK",
                table: "MTPReviews",
                newName: "IX_MTPReviews_MTP_FK");

            migrationBuilder.AddForeignKey(
                name: "FK_MTPReviews_MTPs_MTP_FK",
                table: "MTPReviews",
                column: "MTP_FK",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MTPReviews_MTPs_MTP_FK",
                table: "MTPReviews");

            migrationBuilder.RenameColumn(
                name: "MTP_FK",
                table: "MTPReviews",
                newName: "Client_FK");

            migrationBuilder.RenameIndex(
                name: "IX_MTPReviews_MTP_FK",
                table: "MTPReviews",
                newName: "IX_MTPReviews_Client_FK");

            migrationBuilder.AddForeignKey(
                name: "FK_MTPReviews_MTPs_Client_FK",
                table: "MTPReviews",
                column: "Client_FK",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
