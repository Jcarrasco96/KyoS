using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class BIOinBillWeek : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeBIO",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BilledDate",
                table: "Bio",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeBill",
                table: "Bio",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeniedBill",
                table: "Bio",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Bio",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Units",
                table: "Bio",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeBIO",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "BilledDate",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "CodeBill",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "DeniedBill",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "Units",
                table: "Bio");
        }
    }
}
