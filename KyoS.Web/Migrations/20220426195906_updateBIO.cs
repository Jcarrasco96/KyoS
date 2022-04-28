using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class updateBIO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BioH0031HO",
                table: "Bio",
                newName: "IDAH0031HO");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Bio",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Bio",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Bio");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Bio");

            migrationBuilder.RenameColumn(
                name: "IDAH0031HO",
                table: "Bio",
                newName: "BioH0031HO");
        }
    }
}
