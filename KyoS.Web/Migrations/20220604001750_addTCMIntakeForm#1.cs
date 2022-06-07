using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMIntakeForm1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "TCMIntakeForms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TCMIntakeForms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "TCMIntakeForms",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TCMIntakeForms");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "TCMIntakeForms");
        }
    }
}
