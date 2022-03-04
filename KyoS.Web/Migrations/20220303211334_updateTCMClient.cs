using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class updateTCMClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_TCMClient_TCMClientEntityId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_TCMClientEntityId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IDClient",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "TCMClientEntityId",
                table: "Clients");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "TCMServicePlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "TCMServicePlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "TCMClient",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CaseNumber",
                table: "TCMClient",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "TCMClient",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataClose",
                table: "TCMClient",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DataOpen",
                table: "TCMClient",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "TCMClient",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TCMServicePlans_ClientId",
                table: "TCMServicePlans",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TCMClient_ClientId",
                table: "TCMClient",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMClient_Clients_ClientId",
                table: "TCMClient",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TCMServicePlans_Clients_ClientId",
                table: "TCMServicePlans",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMClient_Clients_ClientId",
                table: "TCMClient");

            migrationBuilder.DropForeignKey(
                name: "FK_TCMServicePlans_Clients_ClientId",
                table: "TCMServicePlans");

            migrationBuilder.DropIndex(
                name: "IX_TCMServicePlans_ClientId",
                table: "TCMServicePlans");

            migrationBuilder.DropIndex(
                name: "IX_TCMClient_ClientId",
                table: "TCMClient");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "TCMServicePlans");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "TCMClient");

            migrationBuilder.DropColumn(
                name: "CaseNumber",
                table: "TCMClient");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "TCMClient");

            migrationBuilder.DropColumn(
                name: "DataClose",
                table: "TCMClient");

            migrationBuilder.DropColumn(
                name: "DataOpen",
                table: "TCMClient");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "TCMClient");

            migrationBuilder.AddColumn<int>(
                name: "IDClient",
                table: "TCMServicePlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TCMClientEntityId",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TCMClientEntityId",
                table: "Clients",
                column: "TCMClientEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_TCMClient_TCMClientEntityId",
                table: "Clients",
                column: "TCMClientEntityId",
                principalTable: "TCMClient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
