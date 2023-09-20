using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMAssessmentImprovesv1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MayWeNA",
                table: "TCMAssessment");

            migrationBuilder.AlterColumn<int>(
                name: "MayWe",
                table: "TCMAssessment",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "MayWe",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "MayWeNA",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
