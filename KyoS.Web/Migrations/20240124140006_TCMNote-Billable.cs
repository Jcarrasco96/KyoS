using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class TCMNoteBillable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Billable",
                table: "TCMNoteActivityTemp",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Billable",
                table: "TCMNoteActivity",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Billable",
                table: "TCMNoteActivityTemp");

            migrationBuilder.DropColumn(
                name: "Billable",
                table: "TCMNoteActivity");
        }
    }
}
