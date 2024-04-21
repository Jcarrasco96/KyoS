using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class DiagnosticV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "DiagnosticsTemp",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateIdentify",
                table: "DiagnosticsTemp",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Prescriber",
                table: "DiagnosticsTemp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Clients_Diagnostics",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateIdentify",
                table: "Clients_Diagnostics",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Prescriber",
                table: "Clients_Diagnostics",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "DiagnosticsTemp");

            migrationBuilder.DropColumn(
                name: "DateIdentify",
                table: "DiagnosticsTemp");

            migrationBuilder.DropColumn(
                name: "Prescriber",
                table: "DiagnosticsTemp");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Clients_Diagnostics");

            migrationBuilder.DropColumn(
                name: "DateIdentify",
                table: "Clients_Diagnostics");

            migrationBuilder.DropColumn(
                name: "Prescriber",
                table: "Clients_Diagnostics");
        }
    }
}
