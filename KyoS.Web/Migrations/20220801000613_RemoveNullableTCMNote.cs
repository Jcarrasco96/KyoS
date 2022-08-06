using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class RemoveNullableTCMNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaseManagerDate",
                table: "TCMNote");

            migrationBuilder.DropColumn(
                name: "DocumentationTime",
                table: "TCMNote");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfService",
                table: "TCMNote",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfService",
                table: "TCMNote",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "CaseManagerDate",
                table: "TCMNote",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DocumentationTime",
                table: "TCMNote",
                type: "datetime2",
                nullable: true);
        }
    }
}
