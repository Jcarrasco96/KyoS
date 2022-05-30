using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMObjetive_review3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TCMObjetives_Clinics_ClinicId",
                table: "TCMObjetives");

            migrationBuilder.DropIndex(
                name: "IX_TCMObjetives_ClinicId",
                table: "TCMObjetives");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "TCMObjetives");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TCMObjetives",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "TCMObjetives");

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "TCMObjetives",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TCMObjetives_ClinicId",
                table: "TCMObjetives",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_TCMObjetives_Clinics_ClinicId",
                table: "TCMObjetives",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
