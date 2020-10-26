using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyClientEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Clinics_ClinicId",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "ClinicId",
                table: "Clients",
                newName: "FacilitatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_ClinicId",
                table: "Clients",
                newName: "IX_Clients_FacilitatorId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Clients",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "MedicalID",
                table: "Clients",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Clients",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Facilitators_FacilitatorId",
                table: "Clients",
                column: "FacilitatorId",
                principalTable: "Facilitators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Facilitators_FacilitatorId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MedicalID",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "FacilitatorId",
                table: "Clients",
                newName: "ClinicId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_FacilitatorId",
                table: "Clients",
                newName: "IX_Clients_ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Clinics_ClinicId",
                table: "Clients",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
