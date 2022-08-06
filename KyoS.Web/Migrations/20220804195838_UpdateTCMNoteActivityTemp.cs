﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateTCMNoteActivityTemp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfServiceOfNote",
                table: "TCMNoteActivityTemp",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IdTCMClient",
                table: "TCMNoteActivityTemp",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfServiceOfNote",
                table: "TCMNoteActivityTemp");

            migrationBuilder.DropColumn(
                name: "IdTCMClient",
                table: "TCMNoteActivityTemp");
        }
    }
}
