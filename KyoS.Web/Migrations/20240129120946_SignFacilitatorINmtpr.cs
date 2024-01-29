using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class SignFacilitatorINmtpr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SignIndTherapy",
                table: "MTPReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SignTherapy",
                table: "MTPReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignIndTherapy",
                table: "MTPReviews");

            migrationBuilder.DropColumn(
                name: "SignTherapy",
                table: "MTPReviews");
        }
    }
}
