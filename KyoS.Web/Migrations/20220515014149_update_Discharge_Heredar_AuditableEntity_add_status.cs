using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class update_Discharge_Heredar_AuditableEntity_add_status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Discharge",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "Discharge",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Discharge",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "Discharge",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discharge_SupervisorId",
                table: "Discharge",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Discharge_Supervisors_SupervisorId",
                table: "Discharge",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discharge_Supervisors_SupervisorId",
                table: "Discharge");

            migrationBuilder.DropIndex(
                name: "IX_Discharge_SupervisorId",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Discharge");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Discharge");
        }
    }
}
