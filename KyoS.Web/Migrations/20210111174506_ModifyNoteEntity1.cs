using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class ModifyNoteEntity1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PlanNote",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Adequate",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AdequateAC",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Anxious",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Congruent",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Depressed",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Dramatized",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Euphoric",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Euthymic",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Fair",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Faulty",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Guarded",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Hostile",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Impaired",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inadequate",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Irritable",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Limited",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MildlyImpaired",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Motivated",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Negativistic",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Normal",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotPerson",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotPlace",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotTime",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Optimistic",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OrientedX3",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Present",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SeverelyImpaired",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShortSpanned",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "Notes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Unmotivated",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Withdrawn",
                table: "Notes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_SupervisorId",
                table: "Notes",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Supervisors_SupervisorId",
                table: "Notes",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Supervisors_SupervisorId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_SupervisorId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Adequate",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "AdequateAC",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Anxious",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Congruent",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Depressed",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Dramatized",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Euphoric",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Euthymic",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Fair",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Faulty",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Guarded",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Hostile",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Impaired",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Inadequate",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Irritable",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Limited",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "MildlyImpaired",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Motivated",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Negativistic",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Normal",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "NotPerson",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "NotPlace",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "NotTime",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Optimistic",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "OrientedX3",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Present",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "SeverelyImpaired",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "ShortSpanned",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Unmotivated",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Withdrawn",
                table: "Notes");

            migrationBuilder.AlterColumn<string>(
                name: "PlanNote",
                table: "Notes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
