using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class IndFacilitatorINmtpr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateIndFacilitator",
                table: "MTPReviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IndFacilitatorId",
                table: "MTPReviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTPReviews_IndFacilitatorId",
                table: "MTPReviews",
                column: "IndFacilitatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_MTPReviews_Facilitators_IndFacilitatorId",
                table: "MTPReviews",
                column: "IndFacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MTPReviews_Facilitators_IndFacilitatorId",
                table: "MTPReviews");

            migrationBuilder.DropIndex(
                name: "IX_MTPReviews_IndFacilitatorId",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "DateIndFacilitator",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "IndFacilitatorId",
                table: "MTPReviews");
        }
    }
}
