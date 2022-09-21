using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class Client_Referred2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "ReferredsTemp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ReferredsTemp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "ReferredsTemp",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ReferredsTemp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "ReferredsTemp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedOn",
                table: "ReferredsTemp",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ReferredsTemp",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferredNote",
                table: "ReferredsTemp",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telephone",
                table: "ReferredsTemp",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "LastModifiedOn",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "ReferredNote",
                table: "ReferredsTemp");

            migrationBuilder.DropColumn(
                name: "Telephone",
                table: "ReferredsTemp");
        }
    }
}
