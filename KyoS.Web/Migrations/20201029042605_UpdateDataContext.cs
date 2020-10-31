using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateDataContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diagnoses_MTPs_MTPId",
                table: "Diagnoses");

            migrationBuilder.AddForeignKey(
                name: "FK_Diagnoses_MTPs_MTPId",
                table: "Diagnoses",
                column: "MTPId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diagnoses_MTPs_MTPId",
                table: "Diagnoses");

            migrationBuilder.AddForeignKey(
                name: "FK_Diagnoses_MTPs_MTPId",
                table: "Diagnoses",
                column: "MTPId",
                principalTable: "MTPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
