using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class CPTcodeInHealthInsurance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CPTcode_BIO",
                table: "HealthInsurances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CPTcode_FARS_MH",
                table: "HealthInsurances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CPTcode_Group",
                table: "HealthInsurances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CPTcode_Ind",
                table: "HealthInsurances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CPTcode_MTP",
                table: "HealthInsurances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CPTcode_MTPR",
                table: "HealthInsurances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CPTcode_PSR",
                table: "HealthInsurances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CPTcode_TCM",
                table: "HealthInsurances",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CPTcode_BIO",
                table: "HealthInsurances");

            migrationBuilder.DropColumn(
                name: "CPTcode_FARS_MH",
                table: "HealthInsurances");

            migrationBuilder.DropColumn(
                name: "CPTcode_Group",
                table: "HealthInsurances");

            migrationBuilder.DropColumn(
                name: "CPTcode_Ind",
                table: "HealthInsurances");

            migrationBuilder.DropColumn(
                name: "CPTcode_MTP",
                table: "HealthInsurances");

            migrationBuilder.DropColumn(
                name: "CPTcode_MTPR",
                table: "HealthInsurances");

            migrationBuilder.DropColumn(
                name: "CPTcode_PSR",
                table: "HealthInsurances");

            migrationBuilder.DropColumn(
                name: "CPTcode_TCM",
                table: "HealthInsurances");
        }
    }
}
