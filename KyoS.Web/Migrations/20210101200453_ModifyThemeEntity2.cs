using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyThemeEntity2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clinics_Themes");

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Themes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Themes_ClinicId",
                table: "Themes",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Themes_Clinics_ClinicId",
                table: "Themes",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Themes_Clinics_ClinicId",
                table: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_Themes_ClinicId",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Themes");

            migrationBuilder.CreateTable(
                name: "Clinics_Themes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThemeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics_Themes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clinics_Themes_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clinics_Themes_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_Themes_ClinicId",
                table: "Clinics_Themes",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_Themes_ThemeId",
                table: "Clinics_Themes",
                column: "ThemeId");
        }
    }
}
