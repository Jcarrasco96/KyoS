using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AuditoryInMedicalHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "IntakeMedicalHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "IntakeMedicalHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "IntakeMedicalHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "IntakeMedicalHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "IntakeMedicalHistory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "IntakeMedicalHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "IntakeMedicalHistory");
        }
    }
}
