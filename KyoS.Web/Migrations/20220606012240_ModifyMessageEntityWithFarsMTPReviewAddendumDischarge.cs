using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyMessageEntityWithFarsMTPReviewAddendumDischarge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddendumId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DischargeId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FarsFormId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MTPReviewId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AddendumId",
                table: "Messages",
                column: "AddendumId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DischargeId",
                table: "Messages",
                column: "DischargeId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_FarsFormId",
                table: "Messages",
                column: "FarsFormId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MTPReviewId",
                table: "Messages",
                column: "MTPReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Adendums_AddendumId",
                table: "Messages",
                column: "AddendumId",
                principalTable: "Adendums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Discharge_DischargeId",
                table: "Messages",
                column: "DischargeId",
                principalTable: "Discharge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_FarsForm_FarsFormId",
                table: "Messages",
                column: "FarsFormId",
                principalTable: "FarsForm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_MTPReviews_MTPReviewId",
                table: "Messages",
                column: "MTPReviewId",
                principalTable: "MTPReviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Adendums_AddendumId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Discharge_DischargeId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_FarsForm_FarsFormId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_MTPReviews_MTPReviewId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_AddendumId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_DischargeId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_FarsFormId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_MTPReviewId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AddendumId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "DischargeId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "FarsFormId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MTPReviewId",
                table: "Messages");
        }
    }
}
