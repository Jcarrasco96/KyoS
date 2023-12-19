﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ArrosConMango : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TCMServices",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4);

            migrationBuilder.AddColumn<bool>(
                name: "Younger",
                table: "TCMClient",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TCMIntakeAppendixI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TcmClient_FK = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionedFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupervisorSignatureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Approved = table.Column<int>(type: "int", nullable: false),
                    TcmSupervisorId = table.Column<int>(type: "int", nullable: true),
                    IsEnrolled = table.Column<bool>(type: "bit", nullable: false),
                    HasAmental2 = table.Column<bool>(type: "bit", nullable: false),
                    RequiresServices = table.Column<bool>(type: "bit", nullable: false),
                    Lacks = table.Column<bool>(type: "bit", nullable: false),
                    RequiresOngoing = table.Column<bool>(type: "bit", nullable: false),
                    HasAmental6 = table.Column<bool>(type: "bit", nullable: false),
                    IsInOut = table.Column<bool>(type: "bit", nullable: false),
                    IsNot = table.Column<bool>(type: "bit", nullable: false),
                    HasRecolated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCMIntakeAppendixI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TCMIntakeAppendixI_TCMClient_TcmClient_FK",
                        column: x => x.TcmClient_FK,
                        principalTable: "TCMClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TCMIntakeAppendixI_TCMSupervisors_TcmSupervisorId",
                        column: x => x.TcmSupervisorId,
                        principalTable: "TCMSupervisors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeAppendixI_TcmClient_FK",
                table: "TCMIntakeAppendixI",
                column: "TcmClient_FK",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMIntakeAppendixI_TcmSupervisorId",
                table: "TCMIntakeAppendixI",
                column: "TcmSupervisorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TCMIntakeAppendixI");

            migrationBuilder.DropColumn(
                name: "Younger",
                table: "TCMClient");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TCMServices",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);
        }
    }
}
