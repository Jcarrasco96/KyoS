using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class updateFARSforBILL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BilledDate",
                table: "FarsForm",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeBill",
                table: "FarsForm",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeniedBill",
                table: "FarsForm",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "FarsForm",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Units",
                table: "FarsForm",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CodeFARS",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BilledDate",
                table: "FarsForm");

            migrationBuilder.DropColumn(
                name: "CodeBill",
                table: "FarsForm");

            migrationBuilder.DropColumn(
                name: "DeniedBill",
                table: "FarsForm");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "FarsForm");

            migrationBuilder.DropColumn(
                name: "Units",
                table: "FarsForm");

            migrationBuilder.DropColumn(
                name: "CodeFARS",
                table: "Clinics");

           
        }
    }
}
