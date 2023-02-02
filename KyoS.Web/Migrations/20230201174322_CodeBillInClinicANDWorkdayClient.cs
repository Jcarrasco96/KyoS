using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class CodeBillInClinicANDWorkdayClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeBill",
                table: "Workdays_Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeGroupTherapy",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeIndTherapy",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodePSRTherapy",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeBill",
                table: "Workdays_Clients");

            migrationBuilder.DropColumn(
                name: "CodeGroupTherapy",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "CodeIndTherapy",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "CodePSRTherapy",
                table: "Clinics");
        }
    }
}
