using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyWorkday_Activity_Facilitator1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "activityDailyLiving",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "communityResources",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "copingSkills",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "diseaseManagement",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "healthyLiving",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "lifeSkills",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "relaxationTraining",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "socialSkills",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "stressManagement",
                table: "Workdays_Activities_Facilitators",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "activityDailyLiving",
                table: "Workdays_Activities_Facilitators");

            migrationBuilder.DropColumn(
                name: "communityResources",
                table: "Workdays_Activities_Facilitators");

            migrationBuilder.DropColumn(
                name: "copingSkills",
                table: "Workdays_Activities_Facilitators");

            migrationBuilder.DropColumn(
                name: "diseaseManagement",
                table: "Workdays_Activities_Facilitators");

            migrationBuilder.DropColumn(
                name: "healthyLiving",
                table: "Workdays_Activities_Facilitators");

            migrationBuilder.DropColumn(
                name: "lifeSkills",
                table: "Workdays_Activities_Facilitators");

            migrationBuilder.DropColumn(
                name: "relaxationTraining",
                table: "Workdays_Activities_Facilitators");

            migrationBuilder.DropColumn(
                name: "socialSkills",
                table: "Workdays_Activities_Facilitators");

            migrationBuilder.DropColumn(
                name: "stressManagement",
                table: "Workdays_Activities_Facilitators");
        }
    }
}
