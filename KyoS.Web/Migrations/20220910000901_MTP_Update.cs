using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class MTP_Update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_TCMFarsForm_TCMFarsFormEntityId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_TCMNote_TCMNoteEntityId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_TCMFarsFormEntityId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "TCMFarsFormEntityId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "TCMNoteEntityId",
                table: "Messages",
                newName: "MTPEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_TCMNoteEntityId",
                table: "Messages",
                newName: "IX_Messages_MTPEntityId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TCMObjetives",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TCMDomains",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TCMDomains",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2);

            migrationBuilder.AddColumn<int>(
                name: "DocumentAssistantId",
                table: "MTPs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "MTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupervisorDate",
                table: "MTPs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "MTPs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTPs_DocumentAssistantId",
                table: "MTPs",
                column: "DocumentAssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_MTPs_SupervisorId",
                table: "MTPs",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_MTPs_MTPEntityId",
                table: "Messages",
                column: "MTPEntityId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTPs_DocumentsAssistant_DocumentAssistantId",
                table: "MTPs",
                column: "DocumentAssistantId",
                principalTable: "DocumentsAssistant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MTPs_Supervisors_SupervisorId",
                table: "MTPs",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_MTPs_MTPEntityId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_MTPs_DocumentsAssistant_DocumentAssistantId",
                table: "MTPs");

            migrationBuilder.DropForeignKey(
                name: "FK_MTPs_Supervisors_SupervisorId",
                table: "MTPs");

            migrationBuilder.DropIndex(
                name: "IX_MTPs_DocumentAssistantId",
                table: "MTPs");

            migrationBuilder.DropIndex(
                name: "IX_MTPs_SupervisorId",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "DocumentAssistantId",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "SupervisorDate",
                table: "MTPs");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "MTPs");

            migrationBuilder.RenameColumn(
                name: "MTPEntityId",
                table: "Messages",
                newName: "TCMNoteEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_MTPEntityId",
                table: "Messages",
                newName: "IX_Messages_TCMNoteEntityId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TCMObjetives",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TCMDomains",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TCMDomains",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TCMFarsFormEntityId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_TCMFarsFormEntityId",
                table: "Messages",
                column: "TCMFarsFormEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_TCMFarsForm_TCMFarsFormEntityId",
                table: "Messages",
                column: "TCMFarsFormEntityId",
                principalTable: "TCMFarsForm",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_TCMNote_TCMNoteEntityId",
                table: "Messages",
                column: "TCMNoteEntityId",
                principalTable: "TCMNote",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
