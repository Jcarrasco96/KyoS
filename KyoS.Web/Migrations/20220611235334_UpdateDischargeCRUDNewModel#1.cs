using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class UpdateDischargeCRUDNewModel1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Others_Explain",
                table: "Discharge");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Others_Explain",
                table: "Discharge",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
