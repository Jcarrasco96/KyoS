using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class CitesIndTherapyv11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cite_Clients_ClientId",
                table: "Cite");

            migrationBuilder.DropForeignKey(
                name: "FK_Cite_Clinics_ClinicId",
                table: "Cite");

            migrationBuilder.DropForeignKey(
                name: "FK_Cite_Facilitators_FacilitatorId",
                table: "Cite");

            migrationBuilder.DropForeignKey(
                name: "FK_Cite_Schedule_ScheduleId",
                table: "Cite");

            migrationBuilder.DropForeignKey(
                name: "FK_Cite_Workdays_Clients_Worday_CLientId",
                table: "Cite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cite",
                table: "Cite");

            migrationBuilder.RenameTable(
                name: "Cite",
                newName: "Cites");

            migrationBuilder.RenameIndex(
                name: "IX_Cite_Worday_CLientId",
                table: "Cites",
                newName: "IX_Cites_Worday_CLientId");

            migrationBuilder.RenameIndex(
                name: "IX_Cite_ScheduleId",
                table: "Cites",
                newName: "IX_Cites_ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Cite_FacilitatorId",
                table: "Cites",
                newName: "IX_Cites_FacilitatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Cite_ClinicId",
                table: "Cites",
                newName: "IX_Cites_ClinicId");

            migrationBuilder.RenameIndex(
                name: "IX_Cite_ClientId",
                table: "Cites",
                newName: "IX_Cites_ClientId");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Cites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Cites",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Cites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "Cites",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cites",
                table: "Cites",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cites_Clients_ClientId",
                table: "Cites",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cites_Clinics_ClinicId",
                table: "Cites",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cites_Facilitators_FacilitatorId",
                table: "Cites",
                column: "FacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cites_Schedule_ScheduleId",
                table: "Cites",
                column: "ScheduleId",
                principalTable: "Schedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cites_Workdays_Clients_Worday_CLientId",
                table: "Cites",
                column: "Worday_CLientId",
                principalTable: "Workdays_Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cites_Clients_ClientId",
                table: "Cites");

            migrationBuilder.DropForeignKey(
                name: "FK_Cites_Clinics_ClinicId",
                table: "Cites");

            migrationBuilder.DropForeignKey(
                name: "FK_Cites_Facilitators_FacilitatorId",
                table: "Cites");

            migrationBuilder.DropForeignKey(
                name: "FK_Cites_Schedule_ScheduleId",
                table: "Cites");

            migrationBuilder.DropForeignKey(
                name: "FK_Cites_Workdays_Clients_Worday_CLientId",
                table: "Cites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cites",
                table: "Cites");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Cites");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Cites");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Cites");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "Cites");

            migrationBuilder.RenameTable(
                name: "Cites",
                newName: "Cite");

            migrationBuilder.RenameIndex(
                name: "IX_Cites_Worday_CLientId",
                table: "Cite",
                newName: "IX_Cite_Worday_CLientId");

            migrationBuilder.RenameIndex(
                name: "IX_Cites_ScheduleId",
                table: "Cite",
                newName: "IX_Cite_ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_Cites_FacilitatorId",
                table: "Cite",
                newName: "IX_Cite_FacilitatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Cites_ClinicId",
                table: "Cite",
                newName: "IX_Cite_ClinicId");

            migrationBuilder.RenameIndex(
                name: "IX_Cites_ClientId",
                table: "Cite",
                newName: "IX_Cite_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cite",
                table: "Cite",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cite_Clients_ClientId",
                table: "Cite",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cite_Clinics_ClinicId",
                table: "Cite",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cite_Facilitators_FacilitatorId",
                table: "Cite",
                column: "FacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cite_Schedule_ScheduleId",
                table: "Cite",
                column: "ScheduleId",
                principalTable: "Schedule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cite_Workdays_Clients_Worday_CLientId",
                table: "Cite",
                column: "Worday_CLientId",
                principalTable: "Workdays_Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
