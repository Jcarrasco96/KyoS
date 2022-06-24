using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMAssessment6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CantDoItAtAll",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesClientTranspotation",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DoesClientTranspotationExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NeedALot",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedNoHelp",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedSome",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantDoItAtAll",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientTranspotation",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientTranspotationExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NeedALot",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NeedNoHelp",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NeedSome",
                table: "TCMAssessment");
        }
    }
}
