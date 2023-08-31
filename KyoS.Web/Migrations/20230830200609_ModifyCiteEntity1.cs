using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyCiteEntity1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cites_Clinics_ClinicId",
                table: "Cites");

            migrationBuilder.DropIndex(
                name: "IX_Cites_ClinicId",
                table: "Cites");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Cites");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Cites",
                newName: "DateCite");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateCite",
                table: "Cites",
                newName: "Date");

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Cites",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cites_ClinicId",
                table: "Cites",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cites_Clinics_ClinicId",
                table: "Cites",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
