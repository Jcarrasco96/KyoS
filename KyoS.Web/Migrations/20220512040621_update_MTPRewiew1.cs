using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class update_MTPRewiew1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClinicalDirector",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Documents",
                table: "MTPReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LicensedPractitioner",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Therapist",
                table: "MTPReviews",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClinicalDirector",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "Documents",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "LicensedPractitioner",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "Therapist",
                table: "MTPReviews");
        }
    }
}
