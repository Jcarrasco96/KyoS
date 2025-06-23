using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class DataOfCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Company_AccountNumber",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company_Address",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company_Email",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company_Name",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company_Phone",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company_Routing",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company_Zelle",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Company_AccountNumber",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Company_Address",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Company_Email",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Company_Name",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Company_Phone",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Company_Routing",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "Company_Zelle",
                table: "Clinics");
        }
    }
}
