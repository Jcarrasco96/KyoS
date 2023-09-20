using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class AppendixJ : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAMental2",
                table: "TCMIntakeAppendixJ",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasAMental6",
                table: "TCMIntakeAppendixJ",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasRecolated",
                table: "TCMIntakeAppendixJ",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnrolled",
                table: "TCMIntakeAppendixJ",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNotReceiving",
                table: "TCMIntakeAppendixJ",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Lacks",
                table: "TCMIntakeAppendixJ",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Meets",
                table: "TCMIntakeAppendixJ",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresOngoing",
                table: "TCMIntakeAppendixJ",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresServices",
                table: "TCMIntakeAppendixJ",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAMental2",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "HasAMental6",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "HasRecolated",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "IsEnrolled",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "IsNotReceiving",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "Lacks",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "Meets",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "RequiresOngoing",
                table: "TCMIntakeAppendixJ");

            migrationBuilder.DropColumn(
                name: "RequiresServices",
                table: "TCMIntakeAppendixJ");
        }
    }
}
