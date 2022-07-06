using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMAssessment7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RecommendedOtherSpecify",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSignatureCaseManager",
                table: "TCMAssessment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSignatureTCMSupervisor",
                table: "TCMAssessment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "HoweverOn",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HoweverVisitScheduler",
                table: "TCMAssessment",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TCMAssessment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TCMSupervisorId",
                table: "TCMAssessment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMAssessment_TCMSupervisorId",
                table: "TCMAssessment",
                column: "TCMSupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMAssessment_TCMSupervisors_TCMSupervisorId",
                table: "TCMAssessment",
                column: "TCMSupervisorId",
                principalTable: "TCMSupervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMAssessment_TCMSupervisors_TCMSupervisorId",
                table: "TCMAssessment");

            migrationBuilder.DropIndex(
                name: "IX_TCMAssessment_TCMSupervisorId",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DateSignatureCaseManager",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DateSignatureTCMSupervisor",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HoweverOn",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HoweverVisitScheduler",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TCMSupervisorId",
                table: "TCMAssessment");

            migrationBuilder.AlterColumn<bool>(
                name: "RecommendedOtherSpecify",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
