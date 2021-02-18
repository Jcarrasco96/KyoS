using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyActivityEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Activities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfApprove",
                table: "Activities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacilitatorId",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Activities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_FacilitatorId",
                table: "Activities",
                column: "FacilitatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SupervisorId",
                table: "Activities",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Facilitators_FacilitatorId",
                table: "Activities",
                column: "FacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Supervisors_SupervisorId",
                table: "Activities",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Facilitators_FacilitatorId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Supervisors_SupervisorId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_FacilitatorId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_SupervisorId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "DateOfApprove",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "FacilitatorId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Activities");
        }
    }
}
