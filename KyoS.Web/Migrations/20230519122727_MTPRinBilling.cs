using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class MTPRinBilling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BilledDate",
                table: "MTPReviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeBill",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeniedBill",
                table: "MTPReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "MTPReviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Units",
                table: "MTPReviews",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BilledDate",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "CodeBill",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "DeniedBill",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "Units",
                table: "MTPReviews");
        }
    }
}
