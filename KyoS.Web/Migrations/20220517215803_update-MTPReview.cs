using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class updateMTPReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MTPReviews_MTPs_MTP_FK",
                table: "MTPReviews");

            migrationBuilder.DropIndex(
                name: "IX_MTPReviews_MTP_FK",
                table: "MTPReviews");

            migrationBuilder.AddColumn<int>(
                name: "MtpId",
                table: "MTPReviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Compliment",
                table: "Goals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Compliment_Date",
                table: "Goals",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Compliment_Explain",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Compliment_IdMTPReview",
                table: "Goals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdMTPReview",
                table: "Goals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MTPReviews_MtpId",
                table: "MTPReviews",
                column: "MtpId");

            migrationBuilder.AddForeignKey(
                name: "FK_MTPReviews_MTPs_MtpId",
                table: "MTPReviews",
                column: "MtpId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MTPReviews_MTPs_MtpId",
                table: "MTPReviews");

            migrationBuilder.DropIndex(
                name: "IX_MTPReviews_MtpId",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "MtpId",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "Compliment",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Compliment_Date",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Compliment_Explain",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Compliment_IdMTPReview",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "IdMTPReview",
                table: "Goals");

            migrationBuilder.CreateIndex(
                name: "IX_MTPReviews_MTP_FK",
                table: "MTPReviews",
                column: "MTP_FK",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MTPReviews_MTPs_MTP_FK",
                table: "MTPReviews",
                column: "MTP_FK",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
