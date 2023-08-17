using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class PainScreenANDColumbiaSuicideInTCMintakeV12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPainScore",
                table: "TCMIntakeColumbiaSuicide");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentPainScore",
                table: "TCMIntakeColumbiaSuicide",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
