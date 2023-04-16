using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class IncludingBillInMTP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BilledDate",
                table: "MTPs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeBill",
                table: "MTPs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeniedBill",
                table: "MTPs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "MTPs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeMTP",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BilledDate",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "CodeBill",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "DeniedBill",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "CodeMTP",
                table: "Clinics");
        }
    }
}
