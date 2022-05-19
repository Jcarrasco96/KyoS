using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class updateObjetive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Compliment",
                table: "Objetives",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Compliment_Date",
                table: "Objetives",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Compliment_Explain",
                table: "Objetives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Compliment_IdMTPReview",
                table: "Objetives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdMTPReview",
                table: "Objetives",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Compliment",
                table: "Objetives");

            migrationBuilder.DropColumn(
                name: "Compliment_Date",
                table: "Objetives");

            migrationBuilder.DropColumn(
                name: "Compliment_Explain",
                table: "Objetives");

            migrationBuilder.DropColumn(
                name: "Compliment_IdMTPReview",
                table: "Objetives");

            migrationBuilder.DropColumn(
                name: "IdMTPReview",
                table: "Objetives");
        }
    }
}
