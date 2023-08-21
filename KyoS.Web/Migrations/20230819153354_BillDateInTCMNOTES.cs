using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class BillDateInTCMNOTES : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BilledDate",
                table: "TCMNote",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeBill",
                table: "TCMNote",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeniedBill",
                table: "TCMNote",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "TCMNote",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BilledDate",
                table: "TCMNote");

            migrationBuilder.DropColumn(
                name: "CodeBill",
                table: "TCMNote");

            migrationBuilder.DropColumn(
                name: "DeniedBill",
                table: "TCMNote");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "TCMNote");
        }
    }
}
