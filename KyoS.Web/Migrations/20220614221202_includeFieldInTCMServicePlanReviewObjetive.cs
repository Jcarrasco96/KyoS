using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class includeFieldInTCMServicePlanReviewObjetive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangesUpdate",
                table: "TCMServicePlanReviewDomainObjectives",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangesUpdate",
                table: "TCMServicePlanReviewDomainObjectives");
        }
    }
}
