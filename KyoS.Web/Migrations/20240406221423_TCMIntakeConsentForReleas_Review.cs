using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KyoS.Web.Migrations
{
    /// <inheritdoc />
    public partial class TCMIntakeConsentForReleas_Review : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InForm_AllofTheAbove",
                table: "TCMIntakeConsentForRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "InForm_Electronic",
                table: "TCMIntakeConsentForRelease",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InForm_AllofTheAbove",
                table: "TCMIntakeConsentForRelease");

            migrationBuilder.DropColumn(
                name: "InForm_Electronic",
                table: "TCMIntakeConsentForRelease");
        }
    }
}
