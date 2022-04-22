using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class IntakeMedicalHistory1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgeOfFirstMenstruation",
                table: "IntakeMedicalHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfLastBreastExam",
                table: "IntakeMedicalHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfLastPelvic",
                table: "IntakeMedicalHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfLastPeriod",
                table: "IntakeMedicalHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UsualDurationOfPeriods",
                table: "IntakeMedicalHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsualIntervalBetweenPeriods",
                table: "IntakeMedicalHistory",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeOfFirstMenstruation",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "DateOfLastBreastExam",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "DateOfLastPelvic",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "DateOfLastPeriod",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "UsualDurationOfPeriods",
                table: "IntakeMedicalHistory");

            migrationBuilder.DropColumn(
                name: "UsualIntervalBetweenPeriods",
                table: "IntakeMedicalHistory");
        }
    }
}
